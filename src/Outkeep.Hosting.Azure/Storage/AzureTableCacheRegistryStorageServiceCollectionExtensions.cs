using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outkeep.Caching;
using System;

namespace Outkeep.Hosting.Azure.Storage
{
    public static class AzureTableCacheRegistryStorageServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureTableCacheRegistryStorage(this IServiceCollection services, Action<AzureTableCacheRegistryStorageOptions> configure)
        {
            return services
                .AddSingleton<AzureTableCacheRegistryStorage>()
                .AddSingleton<ICacheRegistryStorage>(sp => sp.GetService<AzureTableCacheRegistryStorage>())
                .AddSingleton<IHostedService>(sp => sp.GetService<AzureTableCacheRegistryStorage>())
                .AddOptions<AzureTableCacheRegistryStorageOptions>()
                .ValidateDataAnnotations()
                .Configure(configure)
                .Services;
        }
    }
}