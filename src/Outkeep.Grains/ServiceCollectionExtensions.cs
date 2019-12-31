using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Outkeep.Interfaces;
using System;

namespace Outkeep.Grains
{
    public static class ServiceCollectionExtensions
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