using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Server
{
    public static class OutkeepServiceCollectionExtensions
    {
        public static IServiceCollection AddOutkeepServer(this IServiceCollection services)
        {
            return services.AddHostedService<OutkeepServerHostedService>();
        }
    }
}