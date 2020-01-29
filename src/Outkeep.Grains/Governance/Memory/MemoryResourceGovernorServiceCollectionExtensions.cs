using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Runtime;
using System;

namespace Outkeep.Governance.Memory
{
    public static class MemoryResourceGovernorServiceCollectionExtensions
    {
        public static IServiceCollection AddMemoryResourceGovernorAsDefault(this IServiceCollection services, Action<MemoryGovernanceOptions>? configure = null)
        {
            return services.AddMemoryResourceGovernor(OutkeepProviderNames.OutkeepDefault, configure);
        }

        public static IServiceCollection AddMemoryResourceGovernor(this IServiceCollection services, string name, Action<MemoryGovernanceOptions>? configure = null)
        {
            services
                .AddSingleton<IMemoryPressureMonitor, MemoryPressureMonitor>()
                .AddSingleton<MemoryResourceGovernor>()
                .AddSingleton<IHostedService>(sp => sp.GetService<MemoryResourceGovernor>())
                .AddSingletonNamedService<IResourceGovernor>(name, (sp, name) => sp.GetService<MemoryResourceGovernor>())
                .AddOptions<MemoryGovernanceOptions>()
                .ValidateDataAnnotations();

            if (configure != null)
            {
                services.Configure(configure);
            }

            return services;
        }
    }
}