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
        private readonly CacheOptions _options;
        private readonly ISystemClock _clock;

        public CacheDirector(IOptions<CacheOptions> options, ILogger<CacheDirector> logger, ISystemClock clock)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            _buckets = new ConcurrentDictionary<CacheEntry, bool>[3];
            for (var i = 0; i < 3; ++i)
            {
                _buckets[i] = new ConcurrentDictionary<CacheEntry, bool>();
            }
        }

        /// <summary>
        /// The collection of entries managed by this cache.
        /// </summary>
        /// <remarks>
        /// The entries do not hold the cached data themselves.
        /// The cache participants that create the entries are responsible for that.
        /// </remarks>
        private readonly ConcurrentDictionary<string, CacheEntry> _entries = new ConcurrentDictionary<string, CacheEntry>();

        private readonly ConcurrentDictionary<CacheEntry, bool>[] _buckets;

        /// <summary>
        /// Used "space" in the cache.
        /// </summary>
        private long _size;

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <inheritdoc />
        public long Size => Interlocked.Read(ref _size);

        /// <inheritdoc />
        public long Capacity => _options.Capacity;

        /// <inheritdoc />
        public ICacheEntry CreateEntry(string key, long size)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size), Resources.Exception_CacheEntryFor_X_MustHaveSizeGreaterThanZero.Format(key));
            if (size > _options.Capacity) throw new ArgumentOutOfRangeException(nameof(size), Resources.Exception_CacheEntryFor_X_MustHaveSizeLesserThanOrEqualCapacityOf_X.Format(key, _options.Capacity));

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

            // check if we could not allocate enough space
            if (!allocated)
            {
                // early expire this entry in case it is not already
                entry.SetExpired(EvictionCause.Capacity);

                // early invoke any user registered eviction callbacks
                entry.ScheduleEvictionCallback();

                // if we found a previous entry then evict it as well
                // an attempt to add an entry must remove any old entry to avoid keeping stale data
                // however only remove the previous entry if it was the same we found otherwise a concurrent thread already took care of it
                if (previous != null) TryEvictEntry(previous);

                return;
            }

            // attempt to early expire the entry to account for this thread suspending or early timeouts
            if (entry.TrySetExpiredOnTimeout(now))
            {
                // early invoke any user registered eviction callbacks
                entry.ScheduleEvictionCallback();

                // ensure eviction of previous entry regardless
                if (previous != null) TryEvictEntry(previous);

                // rollback the space claim
                Interlocked.Add(ref _size, -entry.Size);

                return;
            }

            // the entry claimed space without contention and did not expire early
            bool added;

            // check if we had found a previous entry
            if (previous is null)
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

                    // notify subscribers that the previous entry was evicted
                    previous.ScheduleEvictionCallback();
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
            if (added)
            {
                // add the entry to the appropriate compaction bucket for future reference
                TryBucket(entry);

                // make a last check to see if the entry has expired in the meantime
                // a different thread could have expired it before this thread could bucket it and therefore failed to remove it from the bucket
                // if that happens then we will hold the entry for longer than necessary until the next expiration scan hits
                // to release memory earlier we clawback the entry from the bucket right now so garbage collection can reclaim it
                if (entry.IsExpired)
                {
                    // early clawback due to early expired entry
                    TryUnbucket(entry);
                }
            }
            else
            {
                // we were not successful
                // a concurrent thread either added or replaced the same entry in a way that this thread could not handle in a graceful way
                // in this case we need we go through the expiration steps early
                // this should be a very rare occurrence if at all as each entry should only be accessed by their own thread-safe grain instance in the first place

                // early expire this entry
                entry.SetExpired(EvictionCause.Replaced);

                // early notify the user
                entry.ScheduleEvictionCallback();

                // rollback the space claim so other entries can claim it
                Interlocked.Add(ref _size, -entry.Size);
            }
        }

        private void TryBucket(CacheEntry entry)
        {
            if (entry.Priority >= CachePriority.NeverRemove) return;

            _buckets[(int)entry.Priority].TryAdd(entry, true);
        }

        private void TryUnbucket(CacheEntry entry)
        {
            if (entry.Priority >= CachePriority.NeverRemove) return;

            _buckets[(int)entry.Priority].TryRemove(entry, out _);
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

                    // check if it overflowed
                    if (newSize < 0)
                    {
                        // nothing we can do here
                        return false;
                    }

                    // it did not overflow but we are still overboard
                    if (newSize > _options.Capacity)
                    {
                        // attempt to evict enough entries to allow this one in
                        if (TryCompact(_options.Capacity - newSize, entry.Priority))
                        {
                            // we released enough space so let the loop run again
                            continue;
                        }

                        // otherwise nothing we can do here
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

                // remove it from its compaction bucket
                TryUnbucket(entry);

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
        public void EvictExpired()
        {
            var now = _clock.UtcNow;

            foreach (var pair in _entries)
            {
                var entry = pair.Value;
                if (entry.TrySetExpiredOnTimeout(now))
                {
                    TryEvictEntry(entry);
                }
            }
        }

        /// <summary>
        /// Evicts enough entries to make room for the given quota and priority level.
        /// Eviction happens in this order:
        ///     1. Enough already expired entries.
        ///     2. If the priority demand is at least <see cref="CachePriority.Low"/>, enough entries from the low priority bucket.
        ///     3. If the priority demand is at least <see cref="CachePriority.Normal"/>, enough entries from the normal priority bucket.
        ///     4. If the priority demand is at least <see cref="CachePriority.High"/>, enough entries from the high priority bucket.
        /// </summary>
        /// <param name="quota">The size demand to meet.</param>
        /// <param name="priority">The priority demand to meet.</param>
        /// <returns>Returns <see cref="true"/> if enough entries were evicted to meet demand, otherwise <see cref="false"/>.</returns>
        /// <remarks>
        /// This method will not evict more than the minimum necessary to meet the demand.
        /// This method will not remove entries with a priority of <see cref="CachePriority.NeverRemove"/> even if the demand priority is also <see cref="CachePriority.NeverRemove"/>.
        /// </remarks>
        private bool TryCompact(long quota, CachePriority priority)
        {
            var now = _clock.UtcNow;

            // evict expired entries from all buckets first
            // we enumerate the buckets as opposed to the main dictionary
            // to help remove possible orphans created due to early expiration during commit
            for (var i = 0; i < _buckets.Length; ++i)
            {
                foreach (var item in _buckets[i])
                {
                    var entry = item.Key;
                    if (entry.TrySetExpiredOnTimeout(now) && TryEvictEntry(entry) && (quota -= entry.Size) <= 0)
                    {
                        return true;
                    }
                }
            }

            // if evicting expired entries was not enough
            // then we must early evict enough entries to meet the quota
            // we only evict entries in buckets of up the priority of the new entry
            var max = (int)(priority == CachePriority.NeverRemove ? CachePriority.High : priority);
            for (var i = 0; i <= max; ++i)
            {
                foreach (var item in _buckets[i])
                {
                    var entry = item.Key;
                    if (TryEvictEntry(entry) && (quota -= entry.Size) <= 0)
                    {
                        return true;
                    }
                }
            }

            // if we got here then we were not able to evict enough entries
            // the cache is maxed out and the user should do something about it
            Log.CacheDirectorCannotCompactToTargetSize(_logger, _options.Capacity);
            return false;
        }

        private static class Log
        {
            private static readonly Action<ILogger, long, Exception?> _cacheDirectorCannotCompactToTargetSisze =
                LoggerMessage.Define<long>(
                    LogLevel.Warning,
                    new EventId(0, nameof(CacheDirectorCannotCompactToTargetSize)),
                    Resources.Log_CacheDirectorCannotCompactToTargetSizeOf_X);

            public static void CacheDirectorCannotCompactToTargetSize(ILogger logger, long target) =>
                _cacheDirectorCannotCompactToTargetSisze(logger, target, null);
        }
    }
}