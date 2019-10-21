using Outkeep.Client;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OutkeepServiceCollectionExtensions
    {
        public static IServiceCollection AddOutkeepClient(this IServiceCollection services, Action<OutkeepClientOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return services
                .AddHostedService<OutkeepClientHostedService>()
                .Configure(configure);
        }
    }
}