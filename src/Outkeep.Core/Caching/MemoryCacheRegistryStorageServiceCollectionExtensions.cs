using Outkeep.Caching;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryCacheRegistryStorageServiceCollectionExtensions
    {
        public static IServiceCollection AddMemoryCacheRegistryStorage(this IServiceCollection services)
        {
            return services.AddSingleton<ICacheRegistryStorage, MemoryCacheRegistryStorage>();
        }
    }
}