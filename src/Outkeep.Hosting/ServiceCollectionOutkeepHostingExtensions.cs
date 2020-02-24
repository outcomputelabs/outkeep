using Orleans.Runtime;
using Outkeep;
using Outkeep.Caching;
using Outkeep.Governance;
using Outkeep.Governance.Memory;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionOutkeepHostingExtensions
    {
        /// <summary>
        /// Adds Outkeep core services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        public static IServiceCollection AddOutkeep(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // add the built-in memory pressure monitor
            services.AddSingleton<IMemoryPressureMonitor, MemoryPressureMonitor>();

            // add weak activation facet
            services.AddSingleton<IWeakActivationStateFactory, WeakActivationStateFactory>();
            services.AddSingleton<IAttributeToFactoryMapper<WeakActivationStateAttribute>, WeakActivationStateAttributeMapper>();

            // add default memory resource governor
            services.AddMemoryResourceGovernor(OutkeepProviderNames.OutkeepMemoryResourceGovernor);

            // add the default system clock
            services.AddSingleton<ISystemClock, SystemClock>();

            // add safe timers
            services.AddSafeTimer();

            // add the default caching storage provider
            services.AddNullGrainStorage(OutkeepProviderNames.OutkeepCache);

            // add validation of cache options
            services.AddOptions<CacheOptions>().ValidateDataAnnotations();

            // add the cache activation context
            services.AddCacheActivationContext();

            // add the cache registry
            services.AddCacheRegistry();

            return services;
        }
    }
}