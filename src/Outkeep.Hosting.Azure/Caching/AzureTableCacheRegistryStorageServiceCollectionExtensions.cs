using Outkeep.Caching;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureTableCacheRegistryStorageServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureTableCacheRegistryStorage(this IServiceCollection services, Action<AzureTableCacheRegistryStorageOptions> configure)
        {
            return services
                .AddSingleton<ICacheRegistryStorage, AzureTableCacheRegistryStorage>()
                .AddOptions<AzureTableCacheRegistryStorageOptions>()
                .Configure(configure)
                .ValidateDataAnnotations()
                .Services;
        }
    }
}