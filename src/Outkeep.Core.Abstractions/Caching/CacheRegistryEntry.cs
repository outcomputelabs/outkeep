using Orleans.Concurrency;

namespace Outkeep.Caching
{
    [Immutable]
    public class CacheRegistryEntry
    {
        public CacheRegistryEntry(string key, long size)
        {
            Key = key;
            Size = size;
        }

        public string Key { get; }

        public long Size { get; }
    }
}