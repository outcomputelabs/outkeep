using Outkeep.Caching;
using Outkeep.Caching.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryCacheRegistryServiceCollectionExtensions
    {
        public static IServiceCollection AddMemoryCacheRegistry(this IServiceCollection services)
        {
            return services.AddSingleton<ICacheRegistry, MemoryCacheRegistry>();
        }
    }
}