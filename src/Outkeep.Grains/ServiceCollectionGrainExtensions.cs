using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Outkeep.Grains;
using Outkeep.Interfaces;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionGrainExtensions
    {
        public static IServiceCollection AddCacheGrainOptions(this IServiceCollection services, Action<CacheGrainOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return services
                .Configure(configure)
                .AddSingleton<IValidateOptions<CacheGrainOptions>, CacheOptionsValidator>();
        }

        public static IServiceCollection AddCacheDirectorGrainService(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddGrainService<CacheDirectorGrainService>()
                .AddSingleton<ICacheDirectorGrainServiceClient, CacheDirectorGrainServiceClient>();
        }
    }
}