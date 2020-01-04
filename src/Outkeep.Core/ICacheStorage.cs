using Outkeep.Core.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    /// <summary>
    /// Abstracts storage providers for the caching component.
    /// </summary>
    public interface ICacheStorage
    {
        /// <summary>
        /// Attempts to read the cache item with the given key from storage.
        /// Returns null if the item is not present in storage.
        /// </summary>
        Task<CacheItem?> ReadAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears the cache item with the given key from storage.
        /// </summary>
        Task ClearAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes the cache item with the given key to storage.
        /// </summary>
        Task WriteAsync(string key, CacheItem item, CancellationToken cancellationToken = default);
    }
}