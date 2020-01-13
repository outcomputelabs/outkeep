using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Outkeep.Core;
using System;
using Xunit;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Outkeep.Hosting.Standalone.Tests
{
    public class OutkeepHostingExtensionsTests
    {
        [Fact]
        public void UseStandaloneClusteringConfiguresOutkeep()
        {
            var services = new ServiceCollection();

            var silo = Mock.Of<ISiloBuilder>();
            Mock.Get(silo)
                .Setup(x => x.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
                .Callback((Action<HostBuilderContext, IServiceCollection> action) => action(null!, services))
                .Returns(silo);

            var outkeep = Mock.Of<IOutkeepServerBuilder>();
            Mock.Get(outkeep)
                .Setup(x => x.ConfigureSilo(It.IsAny<Action<HostBuilderContext, ISiloBuilder>>()))
                .Callback((Action<HostBuilderContext, ISiloBuilder> action) => action(null!, silo))
                .Returns(outkeep);

            var siloPort = 11110;
            var gatewayPort = 31000;
            var serviceId = Guid.NewGuid().ToString();
            var clusterId = Guid.NewGuid().ToString();

            outkeep.UseStandaloneClustering(siloPort, gatewayPort, serviceId, clusterId, false);

            Mock.Get(outkeep).VerifyAll();
            Mock.Get(silo).VerifyAll();

            var provider = services.BuildServiceProvider();

            var endpoints = provider.GetService<IOptions<EndpointOptions>>().Value;
            Assert.Equal(siloPort, endpoints.SiloPort);
            Assert.Equal(gatewayPort, endpoints.GatewayPort);

            var cluster = provider.GetService<IOptions<ClusterOptions>>().Value;
            Assert.Equal(serviceId, cluster.ServiceId);
            Assert.Equal(clusterId, cluster.ClusterId);
        }
    }
}