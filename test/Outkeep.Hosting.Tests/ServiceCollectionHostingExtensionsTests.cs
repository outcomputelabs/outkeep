using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Linq;
using Xunit;

namespace Outkeep.Hosting.Tests
{
    public class ServiceCollectionHostingExtensionsTests
    {
        [Fact]
        public void AddOutkeepServer()
        {
            // arrange
            var services = Mock.Of<IServiceCollection>();
            Mock.Get(services).Setup(x => x.GetEnumerator()).Returns(Enumerable.Empty<ServiceDescriptor>().GetEnumerator());
            Mock.Get(services).Setup(x => x.Add(It.Is<ServiceDescriptor>(v => v.ServiceType == typeof(IHostedService) && v.ImplementationType == typeof(OutkeepServerHostedService) && v.Lifetime == ServiceLifetime.Singleton)));

            // act
            var result = services.AddOutkeepServer();

            // assert
            Assert.Same(services, result);
            Mock.Get(services).VerifyAll();
            Mock.Get(services).VerifyNoOtherCalls();
        }
    }
}