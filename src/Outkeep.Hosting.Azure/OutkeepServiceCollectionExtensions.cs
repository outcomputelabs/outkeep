using Microsoft.Extensions.DependencyInjection;
using System;

namespace Outkeep.Hosting.Azure
{
    public static class OutkeepServiceCollectionExtensions
    {
        public static IServiceCollection AddOutkeepAzureClustering(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services;
        }
    }
}