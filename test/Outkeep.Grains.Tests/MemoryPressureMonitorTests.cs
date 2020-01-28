using Microsoft.Extensions.Options;
using Outkeep.Governance.Memory;
using Outkeep.Grains.Tests.Fakes;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class MemoryPressureMonitorTests
    {
        [Fact]
        public void IsUnderPressureReturnsFalseWhenAvailableMemoryIsUnknown()
        {
            // arrange
            var stats = new FakeHostEnvironmentStatistics();
            var options = new MemoryGovernanceOptions();
            var monitor = new MemoryPressureMonitor(stats, Options.Create(options));

            // act
            var result = monitor.IsUnderPressure;

            // assert
            Assert.False(result);
        }
    }
}