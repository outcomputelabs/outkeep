using Orleans.Concurrency;
using System;

namespace Outkeep.Caching.Memory
{
    [Immutable]
    internal class MemoryCacheRegistryEntity
    {
        public MemoryCacheRegistryEntity(string key, int? size, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration, string? etag)
        {
            Key = key;
            Size = size;
            AbsoluteExpiration = absoluteExpiration;
            SlidingExpiration = slidingExpiration;
            ETag = etag;
        }

        public string Key { get; }
        public int? Size { get; }
        public DateTimeOffset? AbsoluteExpiration { get; }
        public TimeSpan? SlidingExpiration { get; }
        public string? ETag { get; }
    }
}