using Outkeep.Core;
using Outkeep.Grains;
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