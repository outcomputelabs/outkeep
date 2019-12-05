using Microsoft.Extensions.Caching.Distributed;
using Outkeep.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionClientExtensions
    {
        public static IServiceCollection AddOutkeepDistributedCache(this IServiceCollection services)
        {
            return services
                .AddSingleton<IDistributedCache, OutkeepDistributedCache>();
        }

        public static IServiceCollection AddOutkeepClient(this IServiceCollection services)
        {
            return services
                .AddHostedService<OutkeepClientHostedService>();
        }
    }
}