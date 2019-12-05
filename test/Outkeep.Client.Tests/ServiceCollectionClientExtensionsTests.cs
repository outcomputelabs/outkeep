using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Outkeep.Client.Tests
{
    public class ServiceCollectionClientExtensionsTests
    {
        [Fact]
        public void AddOutkeepDistributedCache()
        {
            // arrange
            var services = Mock.Of<IServiceCollection>();
            Mock.Get(services).Setup(x => x.Add(It.Is<ServiceDescriptor>(v => v.ServiceType == typeof(IDistributedCache) && v.ImplementationType == typeof(OutkeepDistributedCache) && v.Lifetime == ServiceLifetime.Singleton)));

            // act
            var result = services.AddOutkeepDistributedCache();

            // assert
            Assert.Same(services, result);
            Mock.Get(services).VerifyAll();
            Mock.Get(services).VerifyNoOtherCalls();
        }
    }
}