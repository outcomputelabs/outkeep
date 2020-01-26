using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Runtime;
using Outkeep.Caching;
using Outkeep.Governance;
using Outkeep.Governance.Memory;

namespace Outkeep
{
    public static class OutkeepServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Outkeep core services that are independant of hosting platform.
        /// </summary>
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            // add the built-in memory pressure monitor
            services.AddSingleton<IMemoryPressureMonitor, MemoryPressureMonitor>();

            // add weak activation facet
            services.AddSingleton<IWeakActivationStateFactory, WeakActivationStateFactory>();
            services.AddSingleton<IAttributeToFactoryMapper<WeakActivationStateAttribute>, WeakActivationStateAttributeMapper>();

            // add default memory resource governor
            services
                .AddSingleton<MemoryResourceGovernor>()
                .AddSingleton<IHostedService>(sp => sp.GetService<MemoryResourceGovernor>())
                .AddSingletonNamedService<IResourceGovernor<ActivityState>>(OutkeepProviderNames.OutkeepMemoryResourceGovernor, (sp, name) => sp.GetService<MemoryResourceGovernor>());

            services.AddSingletonNamedService<IResourceGovernor<ActivityState>, MemoryResourceGovernor>(OutkeepProviderNames.OutkeepMemoryResourceGovernor);

            services
                .AddSingleton<ISystemClock, SystemClock>();

            services
                .AddOptions<CacheGrainOptions>()
                .ValidateDataAnnotations();

            return services;
        }
    }
}