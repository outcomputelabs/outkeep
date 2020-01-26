using Orleans.Runtime;
using Outkeep;
using Outkeep.Core;
using Outkeep.Grains;
using Outkeep.Grains.Governance;
using Outkeep.Grains.Governance.Memory;
using Outkeep.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Outkeep core services that are independant of hosting platform.
        /// </summary>
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddMemoryPressureMonitor();

            // add weak activation facet
            services.AddSingleton<IWeakActivationStateFactory, WeakActivationStateFactory>();
            services.AddSingleton<IAttributeToFactoryMapper<WeakActivationStateAttribute>, WeakActivationStateAttributeMapper>();

            // add default memory resource governor
            services.AddSingletonNamedService<IResourceGovernor<ActivityState>, MemoryResourceGovernor>(OutkeepProviderNames.OutkeepMemoryResourceGovernor);

            services
                .AddHostedService<OutkeepServerHostedService>()
                .AddSingleton<ISystemClock, SystemClock>()
                .AddCacheDirector();

            services
                .AddSingleton<ICacheGrainContext, CacheGrainContext>()
                .AddOptions<CacheGrainOptions>()
                .ValidateDataAnnotations();

            return services;
        }
    }
}