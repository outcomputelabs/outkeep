using Outkeep.Governance.Memory;

namespace Outkeep.Grains.Tests.Fakes
{
    public class FakeMemoryPressureMonitor : IMemoryPressureMonitor
    {
        public bool IsUnderPressure { get; set; }
    }
}