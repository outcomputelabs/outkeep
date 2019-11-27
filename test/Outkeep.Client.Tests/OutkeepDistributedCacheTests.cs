using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Orleans;
using Orleans.Concurrency;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Client.Tests
{
    public class OutkeepDistributedCacheTests
    {
        [Fact]
        public async Task GetAsyncCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).GetAsync() == Task.FromResult(value.AsImmutable()));
            var cache = new OutkeepDistributedCache(factory);

            var result = await cache.GetAsync(key).ConfigureAwait(false);

            Assert.Same(value, result);
            Mock.Get(factory).VerifyAll();
        }

        [Fact]
        public async Task RefreshAsyncCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).RefreshAsync() == Task.CompletedTask);
            var cache = new OutkeepDistributedCache(factory);

            await cache.RefreshAsync(key).ConfigureAwait(false);

            Mock.Get(factory).VerifyAll();
        }

        [Fact]
        public async Task RemoveAsyncCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).RemoveAsync() == Task.CompletedTask);
            var cache = new OutkeepDistributedCache(factory);

            await cache.RemoveAsync(key).ConfigureAwait(false);

            Mock.Get(factory).VerifyAll();
        }

        [Fact]
        public async Task SetAsyncCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow.AddHours(1);
            var slidingExpiration = TimeSpan.FromMinutes(1);
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).SetAsync(value.AsImmutable(), absoluteExpiration, slidingExpiration) == Task.CompletedTask);
            var cache = new OutkeepDistributedCache(factory);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            };
            await cache.SetAsync(key, value, options).ConfigureAwait(false);

            Mock.VerifyAll();
        }
    }
}