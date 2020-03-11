using System;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    internal class RegistryEntry : ICacheRegistryEntry
    {
        private readonly MemoryCacheRegistry _registry;

        public RegistryEntry(string key, MemoryCacheRegistry registry)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
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