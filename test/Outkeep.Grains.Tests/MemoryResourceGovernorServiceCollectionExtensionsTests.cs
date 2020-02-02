using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Orleans.Statistics;
using Outkeep.Governance;
using Outkeep.Governance.Memory;
using Outkeep.Grains.Tests.Fakes;
using Outkeep.Timers;
using Xunit;

namespace Outkeep.Grains.Tests
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
        }
    }
}