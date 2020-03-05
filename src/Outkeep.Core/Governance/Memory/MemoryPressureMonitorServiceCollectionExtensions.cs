using Outkeep.Governance.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryPressureMonitorServiceCollectionExtensions
    {
        public static IServiceCollection AddMemoryPressureMonitor(this IServiceCollection services)
        {
            return services.AddSingleton<IMemoryPressureMonitor, MemoryPressureMonitor>();
        }
    }
}