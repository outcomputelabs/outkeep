using Outkeep.Core.Storage;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryCacheStorageServiceCollectionExtensions
    {
        public static IServiceCollection AddMemoryCacheStorage(this IServiceCollection services)
        {
            return services.AddSingleton<ICacheStorage, MemoryCacheStorage>();
        }
    }
}