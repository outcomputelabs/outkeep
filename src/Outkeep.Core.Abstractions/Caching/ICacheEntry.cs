using System;
using System.Threading.Tasks;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Represents an entry in the cache director.
    /// </summary>
    public interface ICacheEntry<TKey> : IDisposable
        where TKey : notnull
    {
        /// <summary>
        /// Gets the key of the entry.
        /// </summary>
        TKey Key { get; }

        /// <summary>
        /// Gets the size required by this entry.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Indicates whether this entry has expired.
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// Returns a task that completes when this entry is evicted.
        /// </summary>
        Task<CacheEvictionArgs<TKey>> Evicted { get; }

        /// <summary>
        /// Gets or sets the fixed time at which the entry will expire.
        /// </summary>
        DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets the priority for this cache entry.
        /// Entries will lower priority will face eviction first upon compaction.
        /// </summary>
        CachePriority Priority { get; set; }

        /// <summary>
        /// Gets the eviction cause for this cache entry.
        /// </summary>
        EvictionCause EvictionCause { get; }

        /// <summary>
        /// Commits this cache entry to the owning <see cref="ICacheDirector{TKey}"/> instance.
        /// </summary>
        /// <returns>The cache entry itself to allow chaining.</returns>
        ICacheEntry<TKey> Commit();

        /// <summary>
        /// Expires the entry immediately.
        /// This does not evict the entry at call time but does it makes it elegible for eviction when applicable.
        /// </summary>
        void Expire();

        /// <summary>
        /// Marks the entry as expired if it has reached expiration time thresholds.
        /// This does not evict the entry at call time but does it makes it elegible for eviction when applicable.
        /// </summary>
        bool TryExpire(DateTimeOffset now);
    }
}