using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Outkeep.Governance.Memory;
using Outkeep.Grains.Tests.Fakes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class MemoryResourceGovernorTests
    {
        [Fact]
        public async Task CyclesAsync()
        {
            // arrange
            var weakActivationCollectionInterval = TimeSpan.FromSeconds(123);
            var options = new MemoryGovernanceOptions
            {
                WeakActivationCollectionInterval = weakActivationCollectionInterval
            };
            var monitor = new FakeMemoryPressureMonitor();
            var timerFactory = new FakeSafeTimerFactory();

            // act
            using var governor = new MemoryResourceGovernor(Options.Create(options), NullLogger<MemoryResourceGovernor>.Instance, monitor, timerFactory);

            await governor.StartAsync(default).ConfigureAwait(false);

            // assert - timer was scheduled
            var timer = Assert.Single(timerFactory.Timers);
            Assert.NotNull(timer.Callback);
            Assert.Null(timer.State);
            Assert.Equal(weakActivationCollectionInterval, timer.DueTime);
            Assert.Equal(weakActivationCollectionInterval, timer.Period);

            // act
            await governor.StopAsync(default).ConfigureAwait(false);

            // assert - timer was disposed
            Assert.Empty(timerFactory.Timers);
        }
    }
}