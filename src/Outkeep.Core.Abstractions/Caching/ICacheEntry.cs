using System;
using System.Threading;

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
        /// Indicates whether this entry is revoked.
        /// </summary>
        bool IsRevoked { get; }

        /// <summary>
        /// Triggers when the entry is revoked.
        /// </summary>
        CancellationToken Revoked { get; }

        /// <summary>
        /// Gets or sets the priority for this cache entry.
        /// Entries will lower priority will face eviction first upon compaction.
        /// </summary>
        CachePriority Priority { get; set; }

        /// <summary>
        /// Commits this cache entry to the owning <see cref="ICacheDirector{TKey}"/> instance.
        /// </summary>
        /// <returns>The cache entry itself to allow chaining.</returns>
        ICacheEntry<TKey> Commit();

        /// <summary>
        /// Revokes the entry immediately.
        /// </summary>
        void Revoke();
    }
}