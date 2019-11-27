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
    }
}