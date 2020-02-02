using Orleans;
using Orleans.Runtime;
using Outkeep.Governance;
using Outkeep.Grains.Tests.Fakes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Grains.Tests
{
    [Collection(nameof(ClusterCollection))]
    public sealed class WeakActivationStateTests
    {
        private readonly ClusterFixture _fixture;

        public WeakActivationStateTests(ClusterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task EnlistAsyncCallsGovernor()
        {
            // arrange
            var grain = _fixture.Cluster.GrainFactory.GetGrain<IWeakActivationTestGrain>(Guid.NewGuid());
            var governor = (FakeResourceGovernor)_fixture.PrimarySiloServiceProvider.GetRequiredServiceByName<IResourceGovernor>("WeakActivationTestGovernor");

            // act
            await grain.EnlistAsync().ConfigureAwait(false);

            // assert
            Assert.True(governor.Registrations.TryGetValue(grain.AsReference<IWeakActivationExtension>(), out var factor));
            Assert.NotNull(factor);
            Assert.Equal(1, factor!.FakeProperty);
        }

        [Fact]
        public async Task DeactivatingUnregistersFromGovernor()
        {
            // arrange
            var grain = _fixture.Cluster.GrainFactory.GetGrain<IWeakActivationTestGrain>(Guid.NewGuid());

            // act - wake up the grain
            await grain.NoopAsync().ConfigureAwait(false);

            // act - put the grain to sleep
            await grain.SleepAsync().ConfigureAwait(false);

            // act - wait for orleans to sleep the grain
            await Task.Delay(1000).ConfigureAwait(false);

            // assert
            var governor = (FakeResourceGovernor)_fixture.PrimarySiloServiceProvider.GetRequiredServiceByName<IResourceGovernor>("WeakActivationTestGovernor");
            Assert.False(governor.Registrations.TryGetValue(grain.AsReference<IWeakActivationExtension>(), out _));
        }

        [Fact]
        public async Task DeactivatingNoopsIfNotEnlisted()
        {
            // arrange
            var grain = _fixture.Cluster.GrainFactory.GetGrain<IWeakActivationTestGrain>(Guid.NewGuid());
            var governor = (FakeResourceGovernor)_fixture.PrimarySiloServiceProvider.GetRequiredServiceByName<IResourceGovernor>("WeakActivationTestGovernor");

            // act - wake up the grain
            await grain.NoopAsync().ConfigureAwait(false);

            // assert - nothing was registered
            Assert.False(governor.Registrations.TryGetValue(grain.AsReference<IWeakActivationExtension>(), out _));

            // act - put the grain to sleep
            await grain.SleepAsync().ConfigureAwait(false);

            // act - wait for orleans to sleep the grain
            await Task.Delay(1000).ConfigureAwait(false);

            // assert - nothing was registered
            Assert.False(governor.Registrations.TryGetValue(grain.AsReference<IWeakActivationExtension>(), out _));
        }
    }

    public interface IWeakActivationTestGrain : IGrainWithGuidKey
    {
        Task NoopAsync();

        Task SleepAsync();

        Task EnlistAsync();
    }

    public class WeakActivationTestGrain : Grain, IWeakActivationTestGrain
    {
        private readonly IWeakActivationState<FakeWeakActivationFactor> _state;

        public WeakActivationTestGrain([WeakActivationState("WeakActivationTestGovernor")] IWeakActivationState<FakeWeakActivationFactor> state)
        {
            _state = state;
        }

        public Task NoopAsync() => Task.CompletedTask;

        public Task EnlistAsync()
        {
            _state.State.FakeProperty = 1;
            return _state.EnlistAsync();
        }

        public Task SleepAsync()
        {
            DeactivateOnIdle();
            return Task.CompletedTask;
        }
    }
}