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
        public async Task TickClearsOnPriority1()
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

        [Fact]
        public async Task TickClearsOnPriority2()
        {
            // arrange
            var options = new MemoryGovernanceOptions
            {
                GrainDeactivationRatio = 0.6
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
            Mock.Get(mediumActivation).Verify(x => x.DeactivateOnIdleAsync());
            Mock.Get(highActivation).VerifyNoOtherCalls();
        }

        [Fact]
        public async Task TickClearsOnPriority3()
        {
            // arrange
            var options = new MemoryGovernanceOptions
            {
                GrainDeactivationRatio = 0.9
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
            Mock.Get(mediumActivation).Verify(x => x.DeactivateOnIdleAsync());
            Mock.Get(highActivation).Verify(x => x.DeactivateOnIdleAsync());
        }

        [Fact]
        public async Task TickQuitsAfterThreshold()
        {
            // arrange
            var options = new MemoryGovernanceOptions
            {
                GrainDeactivationRatio = 1,
                MaxGrainDeactivationAttempts = 3
            };
            var monitor = new FakeMemoryPressureMonitor
            {
                IsUnderPressure = true
            };
            var timerFactory = new FakeSafeTimerFactory();

            using var governor = new MemoryResourceGovernor(Options.Create(options), NullLogger<MemoryResourceGovernor>.Instance, monitor, timerFactory);
            await governor.StartAsync(default).ConfigureAwait(false);
            var timer = Assert.Single(timerFactory.Timers);

            // arrange - enlist faulty target for deactivation
            var activation = Mock.Of<IWeakActivationExtension>(x => x.DeactivateOnIdleAsync() == Task.FromException(new Exception()));
            var factor = new ActivityState { Priority = ActivityPriority.Low };
            await governor.EnlistAsync(activation, factor).ConfigureAwait(false);

            // assert - activation is registered
            Assert.True(governor.IsEnlisted(activation));

            // act - tick the governing timer
            for (var i = 0; i < options.MaxGrainDeactivationAttempts + 1; ++i)
            {
                await timer.Callback(null).ConfigureAwait(false);
            }

            // assert - deactivation was attempted up to amount of times
            Mock.Get(activation).Verify(x => x.DeactivateOnIdleAsync(), Times.Exactly(options.MaxGrainDeactivationAttempts));

            // assert - activation is no longer enlisted
            Assert.False(governor.IsEnlisted(activation));
        }

        [Fact]
        public void EnlistAsyncThrowsOnNullSubject()
        {
            // arrange
            var options = Options.Create(new MemoryGovernanceOptions());
            var logger = NullLogger<MemoryResourceGovernor>.Instance;
            var monitor = new FakeMemoryPressureMonitor();
            var timers = new FakeSafeTimerFactory();
            using var governor = new MemoryResourceGovernor(options, logger, monitor, timers);

            // assert
            Assert.ThrowsAsync<ArgumentNullException>("subject", () => governor.EnlistAsync(null!, null!));
        }

        [Fact]
        public void EnlistAsyncThrowsOnNullFactor()
        {
            // arrange
            var options = Options.Create(new MemoryGovernanceOptions());
            var logger = NullLogger<MemoryResourceGovernor>.Instance;
            var monitor = new FakeMemoryPressureMonitor();
            var timers = new FakeSafeTimerFactory();
            using var governor = new MemoryResourceGovernor(options, logger, monitor, timers);

            // assert
            var subject = new FakeWeakActivationExtension();
            Assert.ThrowsAsync<ArgumentNullException>("factor", () => governor.EnlistAsync(subject, null!));
        }
    }
}