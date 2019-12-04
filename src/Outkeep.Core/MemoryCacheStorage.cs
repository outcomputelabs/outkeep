using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    public class MemoryCacheStorage : ICacheStorage
    {
        private readonly ConcurrentDictionary<string, CacheItem> storage =
            new ConcurrentDictionary<string, CacheItem>();

        public Task ClearAsync(string key, CancellationToken cancellationToken = default)
        {
            storage.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task<CacheItem?> ReadAsync(string key, CancellationToken cancellationToken = default)
        {
            return storage.TryGetValue(key, out var value)
                ? Task.FromResult<CacheItem?>(value)
                : NotFoundTask;
        }

        public Task WriteAsync(string key, CacheItem item, CancellationToken cancellationToken = default)
        {
            storage[key] = item;
            return Task.CompletedTask;
        }

        private static readonly Task<CacheItem?> NotFoundTask =
            Task.FromResult<CacheItem?>(null);
    }
}