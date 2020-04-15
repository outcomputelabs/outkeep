using Outkeep.Governance.Memory;
using System.Threading;

namespace Outkeep.Core.Tests.Fakes
{
    public class FakeMemoryPressureMonitor : IMemoryPressureMonitor
    {
        private readonly AsyncLocal<bool> _isUnderPressure = new AsyncLocal<bool>();

        public bool IsUnderPressure
        {
            get => _isUnderPressure.Value;
            set => _isUnderPressure.Value = value;
        }
    }
}