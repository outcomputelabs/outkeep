using Outkeep.Governance.Memory;

namespace Outkeep.Core.Tests.Fakes
{
    public class FakeMemoryPressureMonitor : IMemoryPressureMonitor
    {
        public bool IsUnderPressure { get; set; }
    }
}