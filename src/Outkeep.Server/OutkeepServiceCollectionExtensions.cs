using Microsoft.Extensions.DependencyInjection;
using Outkeep.Implementations;

namespace Outkeep.Server
{
    public static class OutkeepServiceCollectionExtensions
    {
        public static IServiceCollection AddOutkeepServer(this IServiceCollection services)
        {
            return services
                .AddHostedService<OutkeepServerHostedService>()
                .AddSingleton<ValidateDistributedCacheOptions>();
        }
    }
}