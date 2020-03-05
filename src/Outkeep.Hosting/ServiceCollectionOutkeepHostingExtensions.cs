using Outkeep;
using Outkeep.Caching;
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

            services
                .AddMemoryPressureMonitor()
                .AddWeakActivationFacet()
                .AddMemoryResourceGovernor(OutkeepProviderNames.OutkeepMemoryResourceGovernor)
                .AddSystemClock()
                .AddSafeTimer();

            // add validation of cache options
            // todo: move this to the cache extensions
            services.AddOptions<CacheOptions>().ValidateDataAnnotations();

            // add the cache activation context
            // todo: move this to the cache extensions
            services.AddCacheActivationContext();

            return services;
        }
    }
}