using System;
using System.Linq;
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

        public Task<CacheRegistryEntry> RegisterOrUpdateAsync(CacheRegistryEntry entry)
        {
            return _storage.WriteAsync(entry);
        }

        public Task<CacheRegistryEntry?> GetAsync(string key)
        {
            return _storage.ReadAsync(key);
        }

        public Task UnregisterAsync(CacheRegistryEntry entry)
        {
            return _storage.ClearAsync(entry);
        }

        public IQueryable<CacheRegistryEntry> CreateQuery()
        {
            return _storage.CreateQuery();
        }
    }
}