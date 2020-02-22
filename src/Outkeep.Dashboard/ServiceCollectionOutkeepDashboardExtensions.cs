using Outkeep.Dashboard;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionOutkeepDashboardExtensions
    {
        public static IServiceCollection AddOutkeepDashboard(this IServiceCollection services, Action<OutkeepDashboardOptions>? configure = null)
        {
            services
                .AddHostedService<OutkeepDashboardService>()
                .AddOptions<OutkeepDashboardOptions>()
                .ValidateDataAnnotations();

            if (configure != null)
            {
                services.Configure(configure);
            }

            return services;
        }
    }
}