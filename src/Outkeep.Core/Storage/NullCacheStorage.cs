using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core.Storage
{
    /// <summary>
    /// Implements a <see cref="ICacheStorage"/> provider that does not store anything.
    /// Use this to disable cache storage altogether.
    /// </summary>
    public class NullCacheStorage : ICacheStorage
    {
        private static readonly Task<CacheItem?> NullTask = Task.FromResult<CacheItem?>(null);

        public Task ClearAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<CacheItem?> ReadAsync(string key, CancellationToken cancellationToken = default)
        {
            return NullTask;
        }

        public Task WriteAsync(string key, CacheItem item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}