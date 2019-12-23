using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outkeep.Core.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core.Caching
{
    [DebuggerDisplay("Count = {Count}, Size = {Size}, Capacity = {Capacity}")]
    internal sealed class CacheDirector : ICacheDirector, ICacheContext
    {
        private readonly ILogger<CacheDirector> _logger;
        private readonly ILogger<CacheEntry> _entryLogger;
        private readonly CacheDirectorOptions _options;
        private readonly ISystemClock _clock;

        public CacheDirector(IOptions<CacheDirectorOptions> options, ILogger<CacheDirector> logger, ILogger<CacheEntry> entryLogger, ISystemClock clock)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _entryLogger = entryLogger ?? throw new ArgumentNullException(nameof(entryLogger));
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
        /// Holds callback registrations.
        /// </summary>
        /// <remarks>
        /// This implementation expects this array to remain stable during runtime with only a few fixed entries added at startup.
        /// Therefore the immutable array here maximizes iteration speed during scheduling while trading-off thread-safety performance during modifications.
        /// </remarks>
        private ImmutableArray<OvercapacityCallbackRegistration> _overcapacityCallbackRegistrations = new ImmutableArray<OvercapacityCallbackRegistration>();

        /// <summary>
        /// This lock is only used to make callback registrations thread-safe.
        /// This implementation expects nil contention on registrations after startup.
        /// </summary>
        private readonly object _lock = new object();

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

            return new CacheEntry(key, size, _entryLogger, this);
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

                // check if we were failed to add the item to the dictionary
                if (!added)
                {
                    // we were not successful
                    // a concurrent thread either added or replaced the same entry in a way that this thread could not handle in a graceful way
                    // in this case we need we go through the expiration steps early
                    // this should be a very rare occurrence if at all as each entry should only be accessed by their own thread-safe grain instance in the first place

                    // free up the space allocation so entries can claim it
                    Interlocked.Add(ref _size, -entry.Size);

                    // expire this entry
                    entry.SetExpired(EvictionCause.Replaced);

                    // notify the user
                    entry.ScheduleEvictionCallback();
                }

                // regardless of how we got here any previous entry was replaced and expired
                // therefore invoke any user registered callbacks on it
                // todo: refactor as to only attempt execution of this method once
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

                    // the cache ran out of space so trigger early compaction
                    ScheduleOvercapacityCompaction();
                }

                // early invoke any user registered eviction callbacks now
                entry.ScheduleEvictionCallback();

                // if we found a previous entry then remove it as well
                // an attempt to add an entry must remove any old entry even if unsuccessful
                if (previous != null)
                {
                    // only remove the previous entry if it was the same we found
                    // otherwise a concurrent thread already took care of it
                    if (_entries.TryRemove(new KeyValuePair<string, CacheEntry>(previous.Key, previous)))
                    {
                        // free up the space taken by the previous entry
                        Interlocked.Add(ref _size, -previous.Size);

                        // we evicted the previous entry so call the user callback
                        previous.ScheduleEvictionCallback();
                    }
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
                    if (newSize < 0 || newSize > _options.Capacity)
                    {
                        // we went overboard either way
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

        private static void ValidateKey(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Removes the given entry from the dictionary and updates the used spaced as appropriate.
        /// Takes no action if the given entry no longer exists in the dictionary.
        /// </summary>
        private void RemoveEntry(CacheEntry entry)
        {
            if (_entries.TryRemove(new KeyValuePair<string, CacheEntry>(entry.Key, entry)))
            {
                // free up the space used by this entry
                Interlocked.Add(ref _size, -entry.Size);

                // expire the entry if not expired yet
                entry.SetExpired(EvictionCause.Removed);

                // notify the user of eviction
                entry.ScheduleEvictionCallback();
            }
        }

        /// <summary>
        /// Called by each entry on self expiry.
        /// </summary>
        public void OnEntryExpired(CacheEntry entry)
        {
            RemoveEntry(entry);
        }

        /// <inheritdoc />
        public void RemoveExpired()
        {
            var now = _clock.UtcNow;

            foreach (var entry in _entries.Values)
            {
                if (entry.TrySetExpiredOnTimeout(now))
                {
                    RemoveEntry(entry);
                }
            }
        }

        [Obsolete("TODO: Move this to the cache director grain")]
        private void ScheduleOvercapacityCompaction()
        {
            Log.SchedulingOverCapacityCompaction(_logger);

            // run in a background process
            // todo: refactor this to run on a non-reentrant timer
            ThreadPool.QueueUserWorkItem(state => OvercapacityCompaction((CacheDirector)state), this);
        }

        [Obsolete("TODO: Move this to the cache director grain")]
        private static void OvercapacityCompaction(CacheDirector manager)
        {
            var currentSize = Interlocked.Read(ref manager._size);

            Log.StartingOvercapacityCompaction(manager._logger, currentSize);

            var threshold = manager._options.Capacity * (1 - manager._options.TargetCompactionRatio);
            if (currentSize > threshold)
            {
                manager.Compact(currentSize - (long)threshold, entry => entry.Size);
            }

            Log.CompletedOvercapacityCompaction(manager._logger, Interlocked.Read(ref manager._size));
        }

        /// Remove at least the given percentage (0.10 for 10%) of the total entries (or estimated memory?), according to the following policy:
        /// 1. Remove all expired items.
        /// 2. Bucket by CacheItemPriority.
        /// 3. Least recently used objects.
        /// ?. Items with the soonest absolute expiration.
        /// ?. Items with the soonest sliding expiration.
        /// ?. Larger objects - estimated by object graph size, inaccurate.
        [Obsolete("TODO: Move this to the cache director grain")]
        public void Compact(double percentage)
        {
            int removalCountTarget = (int)(_entries.Count * percentage);
            Compact(removalCountTarget, _ => 1);
        }

        [Obsolete("TODO: Move this to the cache director grain")]
        private void Compact(long removalSizeTarget, Func<CacheEntry, long> computeEntrySize)
        {
            // todo: can we re-use buffers here to avoid collection allocations?
            var entriesToRemove = new List<CacheEntry>();
            var lowPriEntries = new List<CacheEntry>();
            var normalPriEntries = new List<CacheEntry>();
            var highPriEntries = new List<CacheEntry>();
            long removedSize = 0;

            // bucket items by expiration and priority
            var now = _clock.UtcNow;
            foreach (var entry in _entries.Values)
            {
                if (entry.TrySetExpiredOnTimeout(now))
                {
                    entriesToRemove.Add(entry);
                    removedSize += computeEntrySize(entry);
                }
                else
                {
                    switch (entry.Priority)
                    {
                        case CachePriority.Low:
                            lowPriEntries.Add(entry);
                            break;

                        case CachePriority.Normal:
                            normalPriEntries.Add(entry);
                            break;

                        case CachePriority.High:
                            highPriEntries.Add(entry);
                            break;

                        case CachePriority.NeverRemove:
                            break;

                        default:
                            throw new NotSupportedException(Resources.Exception_ConditionForType_X_WithValue_X_IsNotSupported.Format(nameof(CachePriority), entry.Priority));
                    }
                }
            }

            ExpirePriorityBucket(ref removedSize, removalSizeTarget, computeEntrySize, entriesToRemove, lowPriEntries);
            ExpirePriorityBucket(ref removedSize, removalSizeTarget, computeEntrySize, entriesToRemove, normalPriEntries);
            ExpirePriorityBucket(ref removedSize, removalSizeTarget, computeEntrySize, entriesToRemove, highPriEntries);

            foreach (var entry in entriesToRemove)
            {
                RemoveEntry(entry);
            }
        }

        /// Policy:
        /// 1. Least recently used objects.
        /// ?. Items with the soonest absolute expiration.
        /// ?. Items with the soonest sliding expiration.
        /// ?. Larger objects - estimated by object graph size, inaccurate.
        [Obsolete("TODO: Move this to the cache director grain")]
        private void ExpirePriorityBucket(ref long removedSize, long removalSizeTarget, Func<CacheEntry, long> computeEntrySize, List<CacheEntry> entriesToRemove, List<CacheEntry> priorityEntries)
        {
            // Do we meet our quota by just removing expired entries?
            if (removalSizeTarget <= removedSize)
            {
                // No-op, we've met quota
                return;
            }

            // Expire enough entries to reach our goal
            // TODO: Refine policy

            // LRU
            foreach (var entry in priorityEntries.OrderBy(entry => entry.UtcLastAccessed))
            {
                entry.SetExpired(EvictionCause.Capacity);
                entriesToRemove.Add(entry);
                removedSize += computeEntrySize(entry);

                if (removalSizeTarget <= removedSize)
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public IDisposable RegisterOvercapacityCallback(Action<object?> callback, object? state, TaskScheduler taskScheduler)
        {
            var registration = new OvercapacityCallbackRegistration(callback, state, taskScheduler, this);

            // this implementation assumes registrations to only happen at startup and even then only ever a few
            // therefore it trades off the possible contention of this lock and garbage created by cloning the registrations array on startup
            // in return for maximum performance when iterating a array on each invoke cycle at runtime
            lock (_lock)
            {
                _overcapacityCallbackRegistrations = _overcapacityCallbackRegistrations.Add(registration);
            }

            return registration;
        }

        /// <inheritdoc />
        public void OnOvercapacityCallbackRegistrationDisposed(OvercapacityCallbackRegistration registration)
        {
            // this implementation expects this callback to be rare under normal workload and only run on graceful application shutdown
            lock (_lock)
            {
                // this creates garbage but keeps the invocation loop very fast
                _overcapacityCallbackRegistrations = _overcapacityCallbackRegistrations.Remove(registration);
            }
        }

        /// <summary>
        /// Schedules all registered overcapacity callbacks on their target task schedulers.
        /// </summary>
        private void ScheduleOvercapacityCallbacks()
        {
            // this implementation assumes that scheduling overcapacity callbacks is a far more frequent event than registering them in the first place
            // therefore it optimizes for iterating the registrations in a fast lockless manner by capturing an immutable array
            // this comes at the small expense of having to lock the registrations array when adding or removing items and creating some garbage in the process

            // capture for free thread safety
            var registrations = _overcapacityCallbackRegistrations;

            // schedule each callback on their target task scheduler
            for (var i = 0; i < registrations.Length; ++i)
            {
                registrations[i].Schedule();
            }
        }

        private static class Log
        {
            public static void SchedulingOverCapacityCompaction(ILogger logger) => _schedulingOvercapacityCompaction(logger, null);

            private static readonly Action<ILogger, Exception?> _schedulingOvercapacityCompaction = LoggerMessage.Define(LogLevel.Debug, new EventId(1, nameof(SchedulingOverCapacityCompaction)), Resources.Log_SchedulingOvercapacityCompaction);

            public static void StartingOvercapacityCompaction(ILogger logger, long size) => _startingOvercapacityCompaction(logger, size, null);

            private static readonly Action<ILogger, long, Exception?> _startingOvercapacityCompaction = LoggerMessage.Define<long>(LogLevel.Debug, new EventId(2, nameof(StartingOvercapacityCompaction)), Resources.Log_StartingOvercapacityCompactionWithSizeOf_X);

            public static void CompletedOvercapacityCompaction(ILogger logger, long size) => _completedOvercapacityCompaction(logger, size, null);

            private static readonly Action<ILogger, long, Exception?> _completedOvercapacityCompaction = LoggerMessage.Define<long>(LogLevel.Debug, new EventId(3, nameof(CompletedOvercapacityCompaction)), Resources.Log_CompletedOvercapacityCompactionWithSizeOf_X);
        }
    }
}