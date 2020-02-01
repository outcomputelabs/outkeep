using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Outkeep.Governance;
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

        [Fact]
        public async Task TickNoopsWhenNotUnderPressure()
        {
            // arrange
            var options = new MemoryGovernanceOptions();
            var monitor = new FakeMemoryPressureMonitor
            {
                IsUnderPressure = false
            };
            var timerFactory = new FakeSafeTimerFactory();

            using var governor = new MemoryResourceGovernor(Options.Create(options), NullLogger<MemoryResourceGovernor>.Instance, monitor, timerFactory);
            await governor.StartAsync(default).ConfigureAwait(false);
            var timer = Assert.Single(timerFactory.Timers);

            // act - tick the governing timer
            await timer.Callback(null).ConfigureAwait(false);

            // assert - nothing to test yet
            Assert.True(true);
        }

        [Fact]
        public async Task TickNoopsWhenNothingToDeactivate()
        {
            // arrange
            var options = new MemoryGovernanceOptions();
            var monitor = new FakeMemoryPressureMonitor
            {
                IsUnderPressure = true
            };
            var timerFactory = new FakeSafeTimerFactory();

            using var governor = new MemoryResourceGovernor(Options.Create(options), NullLogger<MemoryResourceGovernor>.Instance, monitor, timerFactory);
            await governor.StartAsync(default).ConfigureAwait(false);
            var timer = Assert.Single(timerFactory.Timers);

            // act - tick the governing timer
            await timer.Callback(null).ConfigureAwait(false);

            // assert - nothing to test yet
            Assert.True(true);
        }

        [Fact]
        public async Task TickNoopsWhenZeroQuota()
        {
            // arrange
            var options = new MemoryGovernanceOptions
            {
                GrainDeactivationRatio = 0
            };
            var monitor = new FakeMemoryPressureMonitor
            {
                IsUnderPressure = true
            };
            var timerFactory = new FakeSafeTimerFactory();

            using var governor = new MemoryResourceGovernor(Options.Create(options), NullLogger<MemoryResourceGovernor>.Instance, monitor, timerFactory);
            await governor.StartAsync(default).ConfigureAwait(false);
            var timer = Assert.Single(timerFactory.Timers);

            // arrange - enlist targets for deactivation
            var lowActivation = Mock.Of<IWeakActivationExtension>();
            var lowFactor = new ActivityState { Priority = ActivityPriority.Low };
            await governor.EnlistAsync(lowActivation, lowFactor).ConfigureAwait(false);

            var mediumActivation = Mock.Of<IWeakActivationExtension>();
            var mediumFactor = new ActivityState { Priority = ActivityPriority.Normal };
            await governor.EnlistAsync(mediumActivation, mediumFactor).ConfigureAwait(false);

            var highActivation = Mock.Of<IWeakActivationExtension>();
            var highFactor = new ActivityState { Priority = ActivityPriority.High };
            await governor.EnlistAsync(highActivation, highFactor).ConfigureAwait(false);

            // act - tick the governing timer
            await timer.Callback(null).ConfigureAwait(false);

            // assert - nothing to test yet
            Mock.Get(lowActivation).VerifyNoOtherCalls();
            Mock.Get(mediumActivation).VerifyNoOtherCalls();
            Mock.Get(highActivation).VerifyNoOtherCalls();
        }

        [Fact]
        public async Task TickClearsOnPriority()
        {
            // arrange
            var options = new MemoryGovernanceOptions
            {
                GrainDeactivationRatio = 0.3
            };
            var monitor = new FakeMemoryPressureMonitor
            {
                IsUnderPressure = true
            };
            var timerFactory = new FakeSafeTimerFactory();

            using var governor = new MemoryResourceGovernor(Options.Create(options), NullLogger<MemoryResourceGovernor>.Instance, monitor, timerFactory);
            await governor.StartAsync(default).ConfigureAwait(false);
            var timer = Assert.Single(timerFactory.Timers);

            // arrange - enlist targets for deactivation
            var lowActivation = Mock.Of<IWeakActivationExtension>();
            var lowFactor = new ActivityState { Priority = ActivityPriority.Low };
            await governor.EnlistAsync(lowActivation, lowFactor).ConfigureAwait(false);

            var mediumActivation = Mock.Of<IWeakActivationExtension>();
            var mediumFactor = new ActivityState { Priority = ActivityPriority.Normal };
            await governor.EnlistAsync(mediumActivation, mediumFactor).ConfigureAwait(false);

            var highActivation = Mock.Of<IWeakActivationExtension>();
            var highFactor = new ActivityState { Priority = ActivityPriority.High };
            await governor.EnlistAsync(highActivation, highFactor).ConfigureAwait(false);

            // act - tick the governing timer
            await timer.Callback(null).ConfigureAwait(false);

            // assert - nothing to test yet
            Mock.Get(lowActivation).Verify(x => x.DeactivateOnIdleAsync());
            Mock.Get(mediumActivation).VerifyNoOtherCalls();
            Mock.Get(highActivation).VerifyNoOtherCalls();
        }
    }
}