using Microsoft.Extensions.DependencyInjection;
using System;

namespace Outkeep.Client
{
    public static class OutkeepServiceCollectionExtensions
    {
        public static IServiceCollection AddOutkeepClient(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddHostedService<OutkeepClientHostedService>();
        }
    }
}