using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outkeep.Core.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Outkeep.Core.Caching
{
    [DebuggerDisplay("Count = {Count}, Size = {Size}, Capacity = {Capacity}")]
    internal sealed class CacheDirector : ICacheDirector, ICacheContext
    {
        private readonly ILogger<CacheDirector> _logger;
        private readonly CacheDirectorOptions _options;
        private readonly ISystemClock _clock;

        public CacheDirector(IOptions<CacheDirectorOptions> options, ILogger<CacheDirector> logger, ILogger<CacheEntry> entryLogger, ISystemClock clock)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// The collection of entries managed by this cache.
        /// </summary>
        /// <remarks>
        /// The entries do not hold the cached data themselves.
        /// The cache participants that create the entries are responsible for that.
        /// </remarks>
        private readonly ConcurrentDictionary<string, CacheEntry> _entries = new ConcurrentDictionary<string, CacheEntry>();

        /// <summary>
        /// Used "space" in the cache.
        /// </summary>
        private long _size;

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <inheritdoc />
        public long Size => Interlocked.Read(ref _size);

        /// <inheritdoc />
        public long TargetCapacity => _options.TargetCapacity;

        /// <inheritdoc />
        public long MaxCapacity => _options.MaxCapacity;

        /// <inheritdoc />
        public ICacheEntry CreateEntry(string key, long size)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size), Resources.Exception_CacheEntryFor_X_MustHaveSizeGreaterThanZero.Format(key));

            // entries with size above the target capacity would just get removed by auto compaction
            if (size > _options.TargetCapacity) throw new ArgumentOutOfRangeException(nameof(size), Resources.Exception_CacheEntryFor_X_MustHaveSizeLesserThanOrEqualCapacityOf_X.Format(key, _options.TargetCapacity));

            return new CacheEntry(key, size, this);
        }

        /// <summary>
        /// Attempts to find and expire the item with the given key.
        /// </summary>
        private CacheEntry TryExpire(string key)
        {
            if (_entries.TryGetValue(key, out var previous))
            {
                previous.SetExpired(EvictionCause.Replaced);
            }
            return previous;
        }

        /// <summary>
        /// Accepts an entry into this cache director.
        /// This method is to be called from <see cref="CacheEntry.Commit"/> after entry configuration.
        /// This is the critical performance path in the director.
        /// </summary>
        /// <param name="entry">The entry to accept.</param>
        public void OnEntryCommitted(CacheEntry entry)
        {
            // keep the current clock so evaluations are consistent
            var now = _clock.UtcNow;

            // setting the entry counts as the first access
            entry.UtcLastAccessed = now;

            // mark any previous entry as expired but do not remove it yet
            // we must keep the entry in place to detect race conditions without locking
            var previous = TryExpire(entry.Key);

            // try to allocate space for the new entry
            var allocated = TryClaimSpace(entry);

            // check if we can add the entry
            if (!entry.TrySetExpiredOnTimeout(now) && allocated)
            {
                // the entry did not expire early and it claimed space without contention
                bool added;

                // check if we had found a previous entry
                if (previous == null)
                {
                    // if we found no previous entry then we can try to add the current one as a new one
                    // this will fail if a concurrent thread added an entry with the same key before the current thread got here
                    added = _entries.TryAdd(entry.Key, entry);
                }
                else
                {
                    // if we found a previous entry then we can try to replace it in-place with a check
                    // this will fail if a concurrent thread replaced the previous entry before the current thread got here
                    added = _entries.TryUpdate(entry.Key, entry, previous);

                    // check if we replaced the entry or faced a concurrency issue
                    if (added)
                    {
                        // we did replace the previous entry
                        // in this case we need to free the space used by that entry
                        Interlocked.Add(ref _size, -previous.Size);
                    }
                    else
                    {
                        // we did not replace the entry
                        // this means a concurrent thread won the race to replace or remove the previous entry before this thread got here
                        // just in case the previous value was removed (and not replaced) we make a best effort attempt to add the new value and win the race
                        added = _entries.TryAdd(entry.Key, entry);
                    }
                }

                // check if we succeeded in adding the entry to the dictionary
                if (!added)
                {
                    // we were not successful
                    // a concurrent thread either added or replaced the same entry in a way that this thread could not handle in a graceful way
                    // in this case we need we go through the expiration steps early
                    // this should be a very rare occurrence if at all as each entry should only be accessed by their own thread-safe grain instance in the first place

                    // rollback the space claim so other entries can claim it
                    Interlocked.Add(ref _size, -entry.Size);

                    // early expire this entry
                    entry.SetExpired(EvictionCause.Replaced);

                    // early notify the user
                    entry.ScheduleEvictionCallback();
                }

                // regardless of how we got here any previous entry was replaced and expired
                // therefore invoke any user registered callbacks on it
                if (previous != null)
                {
                    previous.ScheduleEvictionCallback();
                }
            }
            else
            {
                // the entry either expired early or failed to claim space

                // check if it failed to claim space
                if (!allocated)
                {
                    // early expire this entry in case it is not already
                    entry.SetExpired(EvictionCause.Capacity);
                }

                // early invoke any user registered eviction callbacks now
                entry.ScheduleEvictionCallback();

                // if we found a previous entry then evict it as well
                // an attempt to add an entry must remove any old entry to avoid keeping stale data
                // however only remove the previous entry if it was the same we found otherwise a concurrent thread already took care of it
                if (previous != null)
                {
                    TryEvictEntry(previous);
                }
            }
        }

        /// <summary>
        /// Attempts to allocate space for the given entry.
        /// </summary>
        /// <returns>
        /// Returns <see cref="true"/> if successful, otherwise <see cref="false"/>.
        /// </returns>
        /// <param name="entry">The entry for which to allocate space.</param>
        private bool TryClaimSpace(CacheEntry entry)
        {
            // run a few interlocked attempts to reserve entry size
            // we do this to avoid taking a lock on entire collection
            for (var i = 0; i < 128; ++i)
            {
                // get the current used capacity
                var currentSize = Interlocked.Read(ref _size);

                // compute the future used capacity with the new entry
                unchecked
                {
                    // this can overflow
                    var newSize = currentSize + entry.Size;

                    // check for compute overflow or capacity overflow
                    if (newSize < 0 || newSize > _options.MaxCapacity)
                    {
                        // we went overboard either way

                        // todo: attempt to free the difference

                        return false;
                    }

                    // looks good so lets try to commit the new size
                    if (currentSize == Interlocked.CompareExchange(ref _size, newSize, currentSize))
                    {
                        // we claimed space for this entry by committing the new size
                        return true;
                    }
                }

                // if we got here then a concurrent claim beat this one to it
                // this should be a rare occurrence so try a few more times
            }

            // if we got here then we have exhausted all interlocked attempts
            // this is possible under extreme concurrent load though very unlikely
            // the user can always try again if this is the case
            return false;
        }

        /// <summary>
        /// Removes the given entry from the dictionary, ensures expiration happens, issues callbacks and updates the used spaced as appropriate.
        /// Takes no action if the given entry no longer exists in the dictionary.
        /// </summary>
        /// <returns>
        /// <see cref="true"/> if the specific entry was found and therefore removed, otherwise <see cref="false"/>.
        /// </returns>
        private bool TryEvictEntry(CacheEntry entry)
        {
            if (_entries.TryRemove(new KeyValuePair<string, CacheEntry>(entry.Key, entry)))
            {
                // free up the space used by this entry
                Interlocked.Add(ref _size, -entry.Size);

                // expire the entry if not expired yet
                entry.SetExpired(EvictionCause.Removed);

                // notify the user of eviction
                entry.ScheduleEvictionCallback();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Called by each entry on self expiry.
        /// </summary>
        public void OnEntryExpired(CacheEntry entry)
        {
            TryEvictEntry(entry);
        }

        /// <inheritdoc />
        public void RemoveExpired()
        {
            var now = _clock.UtcNow;

            // todo: attempt to refactor this into context calls
            foreach (var entry in _entries.Values)
            {
                if (entry.TrySetExpiredOnTimeout(now))
                {
                    TryEvictEntry(entry);
                }
            }
        }

        private readonly List<CacheEntry> _compactElectedEntries = new List<CacheEntry>();
        private readonly List<CacheEntry> _compactLowPriorityEntries = new List<CacheEntry>();
        private readonly List<CacheEntry> _compactNormalPriorityEntries = new List<CacheEntry>();
        private readonly List<CacheEntry> _compactHighPriorityEntries = new List<CacheEntry>();

        public void Compact()
        {
            // get the current size without tearing
            var size = Interlocked.Read(ref _size);

            // see how much we need to compact
            var quota = size - _options.TargetCapacity;

            // no-op if we are under target capacity
            if (quota <= 0) return;

            // go through all the entries in a thread-safe fashion
            try
            {
                var now = _clock.UtcNow;
                foreach (var item in _entries)
                {
                    var entry = item.Value;

                    // check if the entry has expired
                    if (entry.TrySetExpiredOnTimeout(now))
                    {
                        // attempt to remove the entry right away
                        if (TryEvictEntry(entry))
                        {
                            quota -= entry.Size;

                            // check if that was enough
                            if (quota <= 0) return;
                        }
                    }
                    else
                    {
                        // put the entry into a priority bucket
                        switch (entry.Priority)
                        {
                            case CachePriority.Low:
                                _compactLowPriorityEntries.Add(entry);
                                break;

                            case CachePriority.Normal:
                                _compactNormalPriorityEntries.Add(entry);
                                break;

                            case CachePriority.High:
                                _compactHighPriorityEntries.Add(entry);
                                break;

                            case CachePriority.NeverRemove:
                                break;

                            default:
                                throw new NotSupportedException(Resources.Exception_ConditionForType_X_WithValue_X_IsNotSupported.Format(nameof(CachePriority), entry.Priority));
                        }
                    }
                }

                // evict each bucket
                if (TryEvictQuota(ref quota, _compactLowPriorityEntries)) return;
                if (TryEvictQuota(ref quota, _compactNormalPriorityEntries)) return;
                if (TryEvictQuota(ref quota, _compactHighPriorityEntries)) return;

                // nope
                _logger.CacheDirectorCannotCompactToTargetSize(_options.TargetCapacity);
            }
            finally
            {
                // clear the buckets so we dont hold on to the entries
                _compactLowPriorityEntries.Clear();
                _compactNormalPriorityEntries.Clear();
                _compactHighPriorityEntries.Clear();
            }
        }

        private bool TryEvictQuota(ref long quota, List<CacheEntry> entries)
        {
            for (var i = 0; i < entries.Count; ++i)
            {
                var entry = entries[i];
                if (TryEvictEntry(entry))
                {
                    quota -= entry.Size;
                    if (quota <= 0) return true;
                }
            }

            return false;
        }
    }
}