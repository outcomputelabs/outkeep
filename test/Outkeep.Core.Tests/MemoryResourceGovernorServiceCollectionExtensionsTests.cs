using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Orleans.Statistics;
using Outkeep.Core.Tests.Fakes;
using Outkeep.Governance;
using Outkeep.Governance.Memory;
using Outkeep.Timers;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class MemoryResourceGovernorServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddMemoryResourceGovernorAsDefaultWorks()
        {
            // arrange
            var services = new ServiceCollection()
                .AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>))
                .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
                .AddSingleton<IHostEnvironmentStatistics, FakeHostEnvironmentStatistics>()
                .AddSingleton<ISafeTimerFactory, FakeSafeTimerFactory>();

            // act
            var result = services.AddMemoryResourceGovernorAsDefault(options =>
            {
                options.LowMemoryBytesThreshold = 123;
            });

            // assert
            Assert.Same(services, result);

            var provider = services.BuildServiceProvider();

            var governor = provider.GetServiceByName<IResourceGovernor>(OutkeepProviderNames.OutkeepDefault);
            Assert.NotNull(governor);
            Assert.IsType<MemoryResourceGovernor>(governor);

            var options = provider.GetService<IOptions<MemoryGovernanceOptions>>().Value;
            Assert.Equal(123, options.LowMemoryBytesThreshold);

            var monitor = provider.GetService<IMemoryPressureMonitor>();
            Assert.NotNull(monitor);
            Assert.IsType<MemoryPressureMonitor>(monitor);

            var hosted = provider.GetService<IHostedService>();
            Assert.Same(governor, hosted);
        }
    }
}