using Outkeep.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionHostingExtensions
    {
        public static IServiceCollection AddOutkeepServer(this IServiceCollection services)
        {
            return services
                .AddHostedService<OutkeepServerHostedService>();
        }
    }
}