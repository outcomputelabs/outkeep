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

            // act
            await grain.NoopAsync().ConfigureAwait(false);

            // assert
            var governor = (FakeResourceGovernor)_fixture.PrimarySiloServiceProvider.GetRequiredServiceByName<IResourceGovernor>("WeakActivationTestGovernor");
            Assert.True(governor.Registrations.TryGetValue(grain.AsReference<IWeakActivationExtension>(), out var factor));
            Assert.NotNull(factor);
            Assert.Equal(1, factor!.FakeProperty);
        }
    }

    public interface IWeakActivationTestGrain : IGrainWithGuidKey
    {
        Task NoopAsync();
    }

    public class WeakActivationTestGrain : Grain, IWeakActivationTestGrain
    {
        private readonly IWeakActivationState<FakeWeakActivationFactor> _state;

        public WeakActivationTestGrain([WeakActivationState("WeakActivationTestGovernor")] IWeakActivationState<FakeWeakActivationFactor> state)
        {
            _state = state;
        }

        public override Task OnActivateAsync()
        {
            _state.State.FakeProperty = 1;
            return _state.EnlistAsync();
        }

        public Task NoopAsync() => Task.CompletedTask;
    }
}