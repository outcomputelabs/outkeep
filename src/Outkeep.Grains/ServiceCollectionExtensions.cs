using Outkeep.Grains;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheGrainOptions(this IServiceCollection services, Action<CacheGrainOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return services
                .Configure(configure)
                .AddSingleton<CacheOptionsValidator>();
        }
    }
}