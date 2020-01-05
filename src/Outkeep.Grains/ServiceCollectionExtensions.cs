using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Outkeep.Interfaces;
using System;

namespace Outkeep.Grains
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheDirectorGrainService(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddGrainService<CacheDirectorGrainService>()
                .AddSingleton<ICacheDirectorGrainServiceClient, CacheDirectorGrainServiceClient>();
        }
    }
}