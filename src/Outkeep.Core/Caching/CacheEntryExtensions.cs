using System;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Quality-of-life extensions for <see cref="ICacheEntry"/>.
    /// </summary>
    internal static class CacheEntryExtensions
    {
        /// <summary>
        /// Sets the absolute expiration for this cache entry.
        /// </summary>
        /// <typeparam name="TKey">The type of key of this cache entry.</typeparam>
        /// <param name="entry">The cache entry instance upon which to set the absolute expiration.</param>
        /// <param name="absoluteExpiration">The absolute expiration to set upon the cache entry.</param>
        /// <returns>The same cache entry instance to allow chaining.</returns>
        public static ICacheEntry SetAbsoluteExpiration(this ICacheEntry entry, DateTime absoluteExpiration)
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
        public static ICacheEntry SetSlidingExpiration(this ICacheEntry entry, TimeSpan slidingExpiration)
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
        public static ICacheEntry SetPriority(this ICacheEntry entry, CachePriority priority)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            entry.Priority = priority;

            return entry;
        }
    }
}