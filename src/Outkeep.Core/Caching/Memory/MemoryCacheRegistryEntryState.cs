using System;

namespace Outkeep.Caching.Memory
{
    internal class MemoryCacheRegistryEntryState : ICacheRegistryEntryState
    {
        public MemoryCacheRegistryEntryState(string key)
        {
            Key = key;
        }

        public string Key { get; }
        public string? ETag { get; set; }
        public int? Size { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
    }
}