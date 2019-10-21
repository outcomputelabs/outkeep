using Outkeep.Implementations;
using Outkeep.Server;

namespace Microsoft.Extensions.DependencyInjection
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