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

        public IQueryable<ICacheRegistryEntry> CreateQuery()
        {
            throw new NotImplementedException();
        }

        public async Task<ICacheRegistryEntry> GetAsync(string key)
        {
            var entry = new CacheRegistryEntry(this, key);

            await _storage.ReadStateAsync(entry).ConfigureAwait(false);

            return entry;
        }

        private Task ClearStateAsync(CacheRegistryEntry entry)
        {
            return _storage.ClearStateAsync(entry);
        }

        private Task ReadStateAsync(CacheRegistryEntry entry)
        {
            return _storage.ReadStateAsync(entry);
        }

        private Task WriteStateAsync(CacheRegistryEntry entry)
        {
            return _storage.WriteStateAsync(entry);
        }

        /// <summary>
        /// Models a cache entry as managed by a <see cref="ICacheRegistryStorage"/> implementation.
        /// </summary>
        private class CacheRegistryEntry : ICacheRegistryEntry, ICacheRegistryEntryState
        {
            private readonly CacheRegistry _registry;

            public CacheRegistryEntry(CacheRegistry registry, string key)
            {
                _registry = registry ?? throw new ArgumentNullException(nameof(registry));

                Key = key ?? throw new ArgumentNullException(nameof(key));
            }

            public string Key { get; }

            public string? ETag { get; set; }

            public int? Size { get; set; }

            public DateTimeOffset? AbsoluteExpiration { get; set; }

            public TimeSpan? SlidingExpiration { get; set; }

            public Task ClearStateAsync()
            {
                return _registry.ClearStateAsync(this);
            }

            public Task ReadStateAsync()
            {
                return _registry.ReadStateAsync(this);
            }

            public Task WriteStateAsync()
            {
                return _registry.WriteStateAsync(this);
            }
        }
    }
}