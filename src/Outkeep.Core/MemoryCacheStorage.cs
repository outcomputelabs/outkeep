using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    public class MemoryCacheStorage : ICacheStorage
    {
        private readonly ConcurrentDictionary<string, (byte[] Value, DateTimeOffset? AbsoluteExpiration, TimeSpan? SlidingExpiration)> storage =
            new ConcurrentDictionary<string, (byte[] Value, DateTimeOffset? AbsoluteExpiration, TimeSpan? SlidingExpiration)>();

        public Task ClearAsync(string key, CancellationToken cancellationToken = default)
        {
            storage.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task<(byte[] Value, DateTimeOffset? AbsoluteExpiration, TimeSpan? SlidingExpiration)?> TryReadAsync(string key, CancellationToken cancellationToken = default)
        {
            if (storage.TryGetValue(key, out var value))
            {
                return Task.FromResult<(byte[], DateTimeOffset?, TimeSpan?)?>(value);
            }
            return NotFoundTask;
        }

        public Task WriteAsync(string key, byte[] value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration, CancellationToken cancellationToken = default)
        {
            storage[key] = (value, absoluteExpiration, slidingExpiration);
            return Task.CompletedTask;
        }

        private static readonly Task<(byte[], DateTimeOffset?, TimeSpan?)?> NotFoundTask =
            Task.FromResult<(byte[], DateTimeOffset?, TimeSpan?)?>(null);
    }
}