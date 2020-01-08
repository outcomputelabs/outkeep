using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Outkeep.Interfaces;

namespace Outkeep.Grains
{
    public static class SiloHostBuilderExtensions
    {
        public static ISiloHostBuilder AddCacheDirectorGrainService(this ISiloHostBuilder builder)
        {
            return builder
                .AddGrainService<CacheDirectorGrainService>()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICacheDirectorGrainServiceClient, CacheDirectorGrainServiceClient>();
                });
        }
    }
}