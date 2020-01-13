using Microsoft.Extensions.DependencyInjection;
using Outkeep.Grains;
using Outkeep.Interfaces;

namespace Orleans.Hosting
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