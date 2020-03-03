using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Caching
{
    public static class CacheRegistryServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheRegistry(this IServiceCollection services)
        {
            return services.AddSingleton<ICacheRegistry, CacheRegistry>();
        }
    }
}