using Microsoft.Extensions.Caching.Distributed;
using Orleans;
using Orleans.Concurrency;
using Outkeep.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Client
{
    internal class OutkeepDistributedCache : IDistributedCache
    {
        private readonly IGrainFactory factory;
        private readonly ISystemClock clock;

        public OutkeepDistributedCache(IGrainFactory factory, ISystemClock clock)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public byte[]? Get(string key)
        {
            return GetAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            var result = await factory.GetCacheGrain(key).GetAsync().ConfigureAwait(false);
            return result.Value;
        }

        public void Refresh(string key)
        {
            RefreshAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return factory.GetCacheGrain(key).RefreshAsync();
        }

        public void Remove(string key)
        {
            RemoveAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return factory.GetCacheGrain(key).RemoveAsync();
        }

        public void Set(string key, byte[]? value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task SetAsync(string key, byte[]? value, DistributedCacheEntryOptions options, CancellationToken token = default)
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
                expiration = clock.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }
            else
            {
                expiration = null;
            }

            return factory.GetCacheGrain(key).SetAsync(new Immutable<byte[]?>(value), expiration, options.SlidingExpiration);
        }
    }
}