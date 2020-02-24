using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    internal class CacheRegistry : ICacheRegistry
    {
        private readonly ICacheRegistryStorage _storage;

        public CacheRegistry(ICacheRegistryStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public Task RegisterAsync(string key, int size, CancellationToken cancellationToken = default)
        {
            return _storage.WriteAsync(key, size, cancellationToken);
        }

        public Task UnregisterAsync(string key, CancellationToken cancellationToken = default)
        {
            return _storage.ClearAsync(key, cancellationToken);
        }
    }
}