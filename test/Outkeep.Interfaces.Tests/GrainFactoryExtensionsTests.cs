using Moq;
using Orleans;
using Xunit;

namespace Outkeep.Interfaces.Tests
{
    public class GrainFactoryExtensionsTests
    {
        [Fact]
        public void GetCacheGrainReturnsGrain()
        {
            // arrange
            var key = "SomeKey";
            var grain = Mock.Of<ICacheGrain>();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null) == grain);

            // act
            var result = factory.GetCacheGrain(key);

            // assert
            Assert.Same(grain, result);
        }
    }
}