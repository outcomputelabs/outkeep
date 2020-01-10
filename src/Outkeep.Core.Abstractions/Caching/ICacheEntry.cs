using System;
using System.Threading.Tasks;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Represents an entry in a <see cref="ICacheDirector"/> instance.
    /// </summary>
    public interface ICacheEntry
    {
        /// <summary>
        /// Gets the key of the entry.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the size required by this entry.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Indicates whether this entry has expired.
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// Gets or sets the fixed time at which the entry will expire.
        /// </summary>
        DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets the sliding timespan at which the entry will expire.
        /// </summary>
        TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// Gets or sets the last time at which the entry was accessed.
        /// The user should update this property as appropriate during the lifetime of the application.
        /// This property will impact evaluation by sliding expiration.
        /// </summary>
        DateTimeOffset UtcLastAccessed { get; set; }

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
        /// Sets a delegate to call upon entry eviction.
        /// The entry only supports a single delegate registered at a time.
        /// Calling this method multiple times will result in the entry keeping the last delegate and discarding earlier ones.
        /// </summary>
        ICacheEntry SetPostEvictionCallback(Action<object?> callback, object? state, TaskScheduler taskScheduler, out IDisposable registration);

        /// <summary>
        /// Commits this cache entry to the owning <see cref="ICacheDirector{TKey}"/> instance.
        /// </summary>
        /// <returns>The cache entry itself to allow chaining.</returns>
        ICacheEntry Commit();

        /// <summary>
        /// Expires the entry immediately.
        /// </summary>
        void Expire();
    }
}