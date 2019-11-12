using Microsoft.Extensions.Caching.Distributed;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Client
{
    internal class OutkeepDistributedCache : IDistributedCache
    {
        private readonly IGrainFactory factory;

        public OutkeepDistributedCache(IGrainFactory factory)
        {
            this.factory = factory;
        }

        public byte[] Get(string key)
        {
            return GetAsync(key).Result;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            var result = await factory.GetCacheGrain(key).GetAsync().ConfigureAwait(false);
            return result.Value;
        }

        public void Refresh(string key)
        {
            RefreshAsync(key).Wait();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return factory.GetCacheGrain(key).RefreshAsync();
        }

        public void Remove(string key)
        {
            RemoveAsync(key).Wait();
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return factory.GetCacheGrain(key).RemoveAsync();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).Wait();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null) ThrowKeyNull();
            if (value == null) ThrowValueNull();
            if (options == null) ThrowOptionsNull();

            DateTimeOffset? expiration;

            if (options.AbsoluteExpiration.HasValue)
            {
                expiration = options.AbsoluteExpiration;
            }
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                expiration = DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }
            else
            {
                expiration = null;
            }

            return factory.GetCacheGrain(key).SetAsync(value.AsImmutable(), expiration, options.SlidingExpiration);

            void ThrowKeyNull() => throw new ArgumentNullException(nameof(key));
            void ThrowValueNull() => throw new ArgumentNullException(nameof(value));
            void ThrowOptionsNull() => throw new ArgumentNullException(nameof(options));
        }
    }
}