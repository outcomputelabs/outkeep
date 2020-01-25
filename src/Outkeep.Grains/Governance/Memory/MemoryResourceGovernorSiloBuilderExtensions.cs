using Outkeep.Grains.Governance.Memory;

namespace Orleans.Hosting
{
    public static class MemoryResourceGovernorSiloBuilderExtensions
    {
        public static ISiloBuilder AddMemoryResourceGovernor(this ISiloBuilder services, string name)
        {
            return services.AddGrainService<MemoryResourceGovernor>();
        }
    }
}