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

        [Fact]
        public void IsUnderPressureReturnsTrueWhenAvailableMemoryFallsBelowThreshold()
        {
            // arrange
            var stats = new FakeHostEnvironmentStatistics
            {
                AvailableMemory = 1000
            };
            var options = new MemoryGovernanceOptions
            {
                LowMemoryBytesThreshold = 1200
            };
            var monitor = new MemoryPressureMonitor(stats, Options.Create(options));

            // act
            var result = monitor.IsUnderPressure;

            // assert
            Assert.True(result);
        }

        [Fact]
        public void IsUnderPressureReturnsFalseWhenTotalMemoryNotAvailable()
        {
            // arrange
            var stats = new FakeHostEnvironmentStatistics
            {
                AvailableMemory = 2000
            };
            var options = new MemoryGovernanceOptions
            {
                LowMemoryBytesThreshold = 1000
            };
            var monitor = new MemoryPressureMonitor(stats, Options.Create(options));

            // act
            var result = monitor.IsUnderPressure;

            // assert
            Assert.False(result);
        }

        [Fact]
        public void IsUnderPressureReturnsFalseWhenTotalMemoryIsZero()
        {
            // arrange
            var stats = new FakeHostEnvironmentStatistics
            {
                AvailableMemory = 2000,
                TotalPhysicalMemory = 0
            };
            var options = new MemoryGovernanceOptions
            {
                LowMemoryBytesThreshold = 1000
            };
            var monitor = new MemoryPressureMonitor(stats, Options.Create(options));

            // act
            var result = monitor.IsUnderPressure;

            // assert
            Assert.False(result);
        }

        [Fact]
        public void IsUnderPressureReturnsTrueWhenRatioBelowThreshold()
        {
            // arrange
            var stats = new FakeHostEnvironmentStatistics
            {
                AvailableMemory = 4000,
                TotalPhysicalMemory = 10000
            };
            var options = new MemoryGovernanceOptions
            {
                LowMemoryBytesThreshold = 1000,
                LowMemoryThreshold = 0.5
            };
            var monitor = new MemoryPressureMonitor(stats, Options.Create(options));

            // act
            var result = monitor.IsUnderPressure;

            // assert
            Assert.True(result);
        }
    }
}