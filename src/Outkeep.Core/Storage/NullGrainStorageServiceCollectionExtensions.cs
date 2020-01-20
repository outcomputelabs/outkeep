using Microsoft.Extensions.DependencyInjection;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace Outkeep.Core.Storage
{
    public static class NullGrainStorageServiceCollectionExtensions
    {
        public static IServiceCollection AddNullGrainStorage(this IServiceCollection services, string name)
        {
            return services.AddSingletonNamedService<IGrainStorage, NullGrainStorage>(name);
        }

        public static IServiceCollection AddNullGrainStorageAsDefault(this IServiceCollection services)
        {
            return services.AddNullGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
        }
    }
}