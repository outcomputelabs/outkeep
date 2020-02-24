using Outkeep.Caching;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Storage
{
    /// <summary>
    /// Implements a <see cref="ICacheRegistryStorage"/> that stores registry items in memory.
    /// For use with unit testing and unreliable standalone deployments.
    /// </summary>
    internal class MemoryCacheRegistryStorage : ICacheRegistryStorage
    {
        private readonly ConcurrentDictionary<string, int> _storage = new ConcurrentDictionary<string, int>();

        public Task ClearAsync(string key, CancellationToken cancellationToken = default)
        {
            _storage.TryRemove(key, out _);

            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<CacheRegistryEntry> EnumerateAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var item in _storage)
            {
                await Task.CompletedTask.ConfigureAwait(true);
                yield return new CacheRegistryEntry(item.Key, item.Value);
            }
        }

        public Task WriteAsync(string key, int size, CancellationToken cancellationToken = default)
        {
            _storage.AddOrUpdate(key, size, (k, e) => size);

            return Task.CompletedTask;
        }
    }
}