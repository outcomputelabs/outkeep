using System;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Quality-of-life extensions for <see cref="ICacheEntry"/>.
    /// </summary>
    public static class CacheEntryExtensions
    {
        /// <summary>
        /// Sets the priority for this cache entry.
        /// </summary>
        /// <param name="entry">The cache entry instance upon which to set the priority.</param>
        /// <param name="priority">The priority to set upon the cache entry.</param>
        /// <returns>The same cache entry instance to allow chaining.</returns>
        public static ICacheEntry<TKey> SetPriority<TKey>(this ICacheEntry<TKey> entry, CachePriority priority) where TKey : notnull
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            entry.Priority = priority;

            return entry;
        }
    }
}