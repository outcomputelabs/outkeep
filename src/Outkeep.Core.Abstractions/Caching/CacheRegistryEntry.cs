using Orleans.Concurrency;
using System;

namespace Outkeep.Caching
{
    /// <summary>
    /// Models a cache entry as managed by a <see cref="ICacheRegistryStorage"/> implementation.
    /// </summary>
    [Immutable]
    public class CacheRegistryEntry
    {
        public CacheRegistryEntry(string key, int? size, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration, string? etag = null)
        {
            if (Key is null) throw new ArgumentNullException(nameof(key));

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