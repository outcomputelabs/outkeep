using Microsoft.Extensions.Options;
using Outkeep.Core.Caching;
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
            return services
                .AddHostedService<OutkeepServerHostedService>()
                .AddSingleton<IValidateOptions<CacheGrainOptions>, CacheGrainOptionsValidator>()
                .AddCacheDirector();
        }
    }
}