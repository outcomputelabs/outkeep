using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Orleans;
using Orleans.Concurrency;
using Outkeep.Core;
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
            var tag = Guid.NewGuid();
            var value = Guid.NewGuid().ToByteArray();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).GetAsync() == new ValueTask<CachePulse>(new CachePulse(tag, new Immutable<byte[]?>(value))));
            var clock = NullClock.Default;
            var cache = new OutkeepDistributedCache(factory, clock);

            var result = await cache.GetAsync(key).ConfigureAwait(false);

            Assert.Same(value, result);
            Mock.Get(factory).VerifyAll();
        }

        [Fact]
        public void GetCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).GetAsync() == new ValueTask<CachePulse>(new CachePulse(Guid.NewGuid(), new Immutable<byte[]?>(value))));
            var clock = NullClock.Default;
            var cache = new OutkeepDistributedCache(factory, clock);

            var result = cache.Get(key);

            Assert.Same(value, result);
            Mock.Get(factory).VerifyAll();
        }

        [Fact]
        public async Task RefreshAsyncCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).RefreshAsync() == Task.CompletedTask);
            var clock = NullClock.Default;
            var cache = new OutkeepDistributedCache(factory, clock);

            await cache.RefreshAsync(key).ConfigureAwait(false);

            Mock.Get(factory).VerifyAll();
        }

        [Fact]
        public void RefreshCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).RefreshAsync() == Task.CompletedTask);
            var clock = NullClock.Default;
            var cache = new OutkeepDistributedCache(factory, clock);

            cache.Refresh(key);

            Mock.Get(factory).VerifyAll();
        }

        [Fact]
        public async Task RemoveAsyncCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).RemoveAsync() == Task.CompletedTask);
            var clock = NullClock.Default;
            var cache = new OutkeepDistributedCache(factory, clock);

            await cache.RemoveAsync(key).ConfigureAwait(false);

            Mock.Get(factory).VerifyAll();
        }

        [Fact]
        public void RemoveCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).RemoveAsync() == Task.CompletedTask);
            var clock = NullClock.Default;
            var cache = new OutkeepDistributedCache(factory, clock);

            cache.Remove(key);

            Mock.Get(factory).VerifyAll();
        }

        [Fact]
        public async Task SetAsyncCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow.AddHours(1);
            var slidingExpiration = TimeSpan.FromMinutes(1);
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).SetAsync(new Immutable<byte[]?>(value), absoluteExpiration, slidingExpiration) == Task.CompletedTask);
            var clock = NullClock.Default;
            var cache = new OutkeepDistributedCache(factory, clock);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            };
            await cache.SetAsync(key, value, options).ConfigureAwait(false);

            Mock.VerifyAll();
        }

        [Fact]
        public async Task SetAsyncCallsGrainWithRelativeAbsoluteExpiration()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            var utcNow = DateTimeOffset.UtcNow;
            var absoluteExpiration = utcNow.Add(absoluteExpirationRelativeToNow);
            var slidingExpiration = TimeSpan.FromMinutes(1);
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).SetAsync(new Immutable<byte[]?>(value), absoluteExpiration, slidingExpiration) == Task.CompletedTask);
            var clock = Mock.Of<ISystemClock>(x => x.UtcNow == utcNow);
            var cache = new OutkeepDistributedCache(factory, clock);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
                SlidingExpiration = slidingExpiration
            };
            await cache.SetAsync(key, value, options).ConfigureAwait(false);

            Mock.VerifyAll();
        }

        [Fact]
        public async Task SetAsyncCallsGrainWithNoExpiration()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var slidingExpiration = TimeSpan.FromMinutes(1);
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).SetAsync(new Immutable<byte[]?>(value), null, slidingExpiration) == Task.CompletedTask);
            var clock = NullClock.Default;
            var cache = new OutkeepDistributedCache(factory, clock);

            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration
            };
            await cache.SetAsync(key, value, options).ConfigureAwait(false);

            Mock.VerifyAll();
        }

        [Fact]
        public void SetCallsGrain()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow.AddHours(1);
            var slidingExpiration = TimeSpan.FromMinutes(1);
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).SetAsync(new Immutable<byte[]?>(value), absoluteExpiration, slidingExpiration) == Task.CompletedTask);
            var clock = NullClock.Default;
            var cache = new OutkeepDistributedCache(factory, clock);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            };
            cache.Set(key, value, options);

            Mock.VerifyAll();
        }
    }
}