using Microsoft.Extensions.DependencyInjection;
using Outkeep.Dashboard;
using System;

namespace Outkeep
{
    public static class OutkeepServerBuilderOutkeepDashboardExtensions
    {
        public static IOutkeepServerBuilder UseOutkeepDashboard(this IOutkeepServerBuilder builder, Action<OutkeepDashboardOptions>? configure = null)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddOutkeepDashboard(configure);
            });
        }
    }
}