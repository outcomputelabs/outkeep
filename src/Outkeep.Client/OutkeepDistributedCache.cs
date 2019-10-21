using Microsoft.Extensions.Caching.Distributed;
using Orleans.Concurrency;
using Outkeep.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Client
{
    internal class OutkeepDistributedCache : IDistributedCache
    {
        private readonly IOutkeepClient client;

        public OutkeepDistributedCache(IOutkeepClient client)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public byte[] Get(string key)
        {
            return GetAsync(key).Result;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            var result = await client.GetCacheGrain(key).GetAsync().ConfigureAwait(false);
            return result.Value;
        }

        public void Refresh(string key)
        {
            RefreshAsync(key).Wait();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return client.GetCacheGrain(key).RefreshAsync();
        }

        public void Remove(string key)
        {
            RemoveAsync(key).Wait();
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return client.GetCacheGrain(key).RemoveAsync();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).Wait();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (options == null) throw new ArgumentNullException(nameof(options));

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

            return client.GetCacheGrain(key).SetAsync(value.AsImmutable(), expiration, options.SlidingExpiration);
        }
    }
}