using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime;
using Outkeep.Caching;
using Outkeep.Governance;
using Outkeep.Governance.Memory;

namespace Outkeep.Core
{
    public static class OutkeepServerBuilderHostingExtensions
    {
        public static IOutkeepServerBuilder AddCoreServices(this IOutkeepServerBuilder builder)
        {
            return builder
                .ConfigureServices(services =>
                {
                    // add the built-in memory pressure monitor
                    services.AddSingleton<IMemoryPressureMonitor, MemoryPressureMonitor>();

                    // add weak activation facet
                    services.AddSingleton<IWeakActivationStateFactory, WeakActivationStateFactory>();
                    services.AddSingleton<IAttributeToFactoryMapper<WeakActivationStateAttribute>, WeakActivationStateAttributeMapper>();

                    // add default memory resource governor
                    services.AddMemoryResourceGovernor(OutkeepProviderNames.OutkeepMemoryResourceGovernor);

                    services.AddSingletonNamedService<IResourceGovernor, MemoryResourceGovernor>(OutkeepProviderNames.OutkeepMemoryResourceGovernor);

                    services
                        .AddSingleton<ISystemClock, SystemClock>();

                    services
                        .AddOptions<CacheOptions>()
                        .ValidateDataAnnotations();
                })
                .ConfigureSilo(silo =>
                {
                    silo.AddGrainExtension<IWeakActivationExtension, GrainControlExtension>();
                });
        }
    }
}