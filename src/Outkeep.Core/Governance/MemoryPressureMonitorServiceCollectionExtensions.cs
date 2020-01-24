using Outkeep.Core.Governance;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryPressureMonitorServiceCollectionExtensions
    {
        public static IServiceCollection AddMemoryPressureMonitor(this IServiceCollection services, Action<MemoryPressureOptions>? configure = null)
        {
            services.AddSingleton<IMemoryPressureMonitor, MemoryPressureMonitor>();

            if (configure != null)
            {
                services.Configure(configure);
            }

            return services;
        }
    }
}