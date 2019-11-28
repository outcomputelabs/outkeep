using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
        public class NullHostedService : IHostedService
        {
            public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }

        [Fact]
        public void TryAddHostedServiceAddsHostedService()
        {
            var services = new ServiceCollection();

            services.TryAddHostedService<NullHostedService>();

            Assert.Single(services, x => x.ServiceType == typeof(IHostedService) && x.ImplementationType == typeof(NullHostedService));
        }

        [Fact]
        public void TryAddHostedServiceSkipsAdding()
        {
            var services = new ServiceCollection();
            services.AddHostedService<NullHostedService>();

            services.TryAddHostedService<NullHostedService>();

            Assert.Single(services, x => x.ServiceType == typeof(IHostedService) && x.ImplementationType == typeof(NullHostedService) && x.Lifetime == ServiceLifetime.Singleton);
        }
    }
}