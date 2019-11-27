using Moq;
using Orleans;
using System;
using Xunit;

namespace Outkeep.Interfaces.Tests
{
    public class ClusterClientExtensionsTests
    {
        [Fact]
        public void GetCacheGrainReturnsCacheGrain()
        {
            var grain = Mock.Of<ICacheGrain>();
            var key = Guid.NewGuid().ToString();
            var client = Mock.Of<IClusterClient>(x => x.GetGrain<ICacheGrain>(key, null) == grain);

            var result = client.GetCacheGrain(key);

            Assert.Same(grain, result);
        }
    }
}