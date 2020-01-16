using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Quality-of-life extensions for <see cref="ICacheEntry"/>.
    /// </summary>
    public static class CacheEntryExtensions
    {
        /// <summary>
        /// Sets the <see cref="ICacheEntry.UtcLastAccessed"/> time for this cache entry.
        /// </summary>
        /// <param name="entry">The cache entry.</param>
        /// <param name="utcLastAccessed">The value for the <see cref="ICacheEntry.UtcLastAccessed"/> property.</param>
        /// <returns>The same cache entry instance to allow chaining.</returns>
        public static ICacheEntry<TKey> SetUtcLastAccessed<TKey>(this ICacheEntry<TKey> entry, DateTimeOffset utcLastAccessed) where TKey : notnull
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            entry.UtcLastAccessed = utcLastAccessed;

            return entry;
        }

        /// <summary>
        /// Sets the absolute expiration for this cache entry.
        /// </summary>
        /// <param name="entry">The cache entry instance upon which to set the absolute expiration.</param>
        /// <param name="absoluteExpiration">The absolute expiration to set upon the cache entry.</param>
        /// <returns>The same cache entry instance to allow chaining.</returns>
        public static ICacheEntry<TKey> SetAbsoluteExpiration<TKey>(this ICacheEntry<TKey> entry, DateTimeOffset? absoluteExpiration) where TKey : notnull
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            entry.AbsoluteExpiration = absoluteExpiration;

            return entry;
        }

        /// <summary>
        /// Sets the sliding expiration for this cache entry.
        /// </summary>
        /// <param name="entry">The cache entry instance upon which to set the sliding expiration.</param>
        /// <param name="slidingExpiration">The sliding expiration to set upon the cache entry.</param>
        /// <returns>The same cache entry instance to allow chaining.</returns>
        public static ICacheEntry<TKey> SetSlidingExpiration<TKey>(this ICacheEntry<TKey> entry, TimeSpan? slidingExpiration) where TKey : notnull
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            entry.SlidingExpiration = slidingExpiration;

            return entry;
        }

        /// <summary>
        /// Sets the priority for this cache entry.
        /// </summary>
        /// <param name="entry">The cache entry instance upon which to set the priority.</param>
        /// <param name="priority">The priority to set upon the cache entry.</param>
        /// <returns>The same cache entry instance to allow chaining.</returns>
        public static ICacheEntry<TKey> SetPriority<TKey>(this ICacheEntry<TKey> entry, CachePriority priority) where TKey : notnull
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            entry.Priority = priority;

            return entry;
        }

        /// <summary>
        /// Convenience method to schedule a continuation on the <see cref="ICacheEntry.Evicted"/> property using the current task scheduler.
        /// </summary>
        public static ICacheEntry<TKey> ContinueWithOnEvicted<TKey>(this ICacheEntry<TKey> entry, Action<Task<CacheEvictionArgs<TKey>>, object?> action, object? state, CancellationToken cancellationToken = default) where TKey : notnull
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            entry.Evicted.ContinueWith(action, state, cancellationToken, TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

            return entry;
        }

        /// <summary>
        /// Convenience method to schedule a continuation on the <see cref="ICacheEntry.Evicted"/> property using the current task scheduler.
        /// </summary>
        public static ICacheEntry<TKey> ContinueWithOnEvicted<TKey>(this ICacheEntry<TKey> entry, Action<Task<CacheEvictionArgs<TKey>>> action, CancellationToken cancellationToken = default) where TKey : notnull
        {
            return ContinueWithOnEvicted(entry, action, out _, cancellationToken);
        }

        /// <summary>
        /// Convenience method to schedule a continuation on the <see cref="ICacheEntry.Evicted"/> property using the current task scheduler.
        /// </summary>
        public static ICacheEntry<TKey> ContinueWithOnEvicted<TKey>(this ICacheEntry<TKey> entry, Action<Task<CacheEvictionArgs<TKey>>> action, out Task continuation, CancellationToken cancellationToken = default) where TKey : notnull
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            continuation = entry.Evicted.ContinueWith(action, cancellationToken, TaskContinuationOptions.DenyChildAttach, TaskScheduler.Current);

            return entry;
        }
    }
}