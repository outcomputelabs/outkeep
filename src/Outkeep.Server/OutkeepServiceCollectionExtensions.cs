using Outkeep.Implementations;
using Outkeep.Server;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OutkeepServiceCollectionExtensions
    {
        public static IServiceCollection AddOutkeepServer(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddHostedService<OutkeepServerHostedService>()
                .AddSingleton<DistributedCacheOptionsValidator>();
        }
    }
}