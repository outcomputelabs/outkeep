using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core.Storage
{
    /// <summary>
    /// Implements an <see cref="ICacheStorage"/> that keeps data in memory.
    /// This class is designed to support unit testing and not production workloads.
    /// </summary>
    public class MemoryCacheStorage : ICacheStorage
    {
        /// <summary>
        /// Holds the cache items.
        /// </summary>
        private readonly ConcurrentDictionary<string, CacheItem> storage =
            new ConcurrentDictionary<string, CacheItem>();

        /// <summary>
        /// Caches the null result task to avoid redundant allocations.
        /// </summary>
        private static readonly Task<CacheItem?> NotFoundTask = Task.FromResult<CacheItem?>(null);

        /// <inheritdoc />
        public Task ClearAsync(string key, CancellationToken cancellationToken = default)
        {
            storage.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<CacheItem?> ReadAsync(string key, CancellationToken cancellationToken = default)
        {
            return storage.TryGetValue(key, out var value)
                ? Task.FromResult<CacheItem?>(value)
                : NotFoundTask;
        }

        /// <inheritdoc />
        public Task WriteAsync(string key, CacheItem item, CancellationToken cancellationToken = default)
        {
            storage[key] = item;
            return Task.CompletedTask;
        }
    }
}