namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Represents a cache manager that does not hold the data itself and instead coordinates opt-in participants.
    /// </summary>
    public interface ICacheDirector
    {
        /// <summary>
        /// Creates and returns a new uncomitted cache entry.
        /// To commit the entry to the owning cache director, call <see cref="ICacheEntry.Commit"/> after configuration.
        /// </summary>
        /// <param name="key">The cache entry key.</param>
        /// <param name="size">The size required by the entry.</param>
        /// <returns>The new cache entry in an uncomitted state.</returns>
        ICacheEntry CreateEntry(string key, long size);

        /// <summary>
        /// Gets the number of entries managed by this cache director.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the total size of the entries managed by this cache director.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Get the capacity managed by this cache director.
        /// </summary>
        long Capacity { get; }

        /// <summary>
        /// Scans the cache for expired entries and removes them as appropriate, scheduling callback invocation.
        /// Does not compact the cache.
        /// </summary>
        void EvictExpired();
    }
}