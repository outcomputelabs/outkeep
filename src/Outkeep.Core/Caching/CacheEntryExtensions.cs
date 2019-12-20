using System;
using System.Diagnostics.CodeAnalysis;

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
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static ICacheEntry SetAbsoluteExpiration(this ICacheEntry entry, DateTime absoluteExpiration)
        {
            if (entry == null) ThrowEntryNull();
            entry.AbsoluteExpiration = absoluteExpiration;
            return entry;

            static void ThrowEntryNull() => throw new ArgumentNullException(nameof(entry));
        }

        /// <summary>
        /// Sets the sliding expiration for this cache entry.
        /// </summary>
        /// <typeparam name="TKey">The type of key of this cache entry.</typeparam>
        /// <param name="entry">The cache entry instance upon which to set the sliding expiration.</param>
        /// <param name="slidingExpiration">The sliding expiration to set upon the cache entry.</param>
        /// <returns>The same cache entry instance to allow chaining.</returns>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static ICacheEntry SetSlidingExpiration<TKey>(this ICacheEntry entry, TimeSpan slidingExpiration)
        {
            if (entry == null) ThrowEntryNull();
            entry.SlidingExpiration = slidingExpiration;
            return entry;

            static void ThrowEntryNull() => throw new ArgumentNullException(nameof(entry));
        }

        /// <summary>
        /// Sets the priority for this cache entry.
        /// </summary>
        /// <typeparam name="TKey">The type of key of this cache entry.</typeparam>
        /// <param name="entry">The cache entry instance upon which to set the priority.</param>
        /// <param name="priority">The priority to set upon the cache entry.</param>
        /// <returns>The same cache entry instance to allow chaining.</returns>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static ICacheEntry SetPriority<TKey>(this ICacheEntry entry, CachePriority priority)
        {
            if (entry == null) ThrowEntryNull();
            entry.Priority = priority;
            return entry;

            static void ThrowEntryNull() => throw new ArgumentNullException(nameof(entry));
        }
    }
}