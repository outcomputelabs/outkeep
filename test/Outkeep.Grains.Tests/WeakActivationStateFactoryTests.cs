using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Outkeep.Governance;
using Outkeep.Grains.Tests.Fakes;
using Outkeep.Properties;
using System;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class WeakActivationStateFactoryTests
    {
        [Fact]
        public void ThrowsOnNullContext()
        {
            // arrange
            var factory = new WeakActivationStateFactory();

            // act
            void action() => factory.Create<FakeWeakActivationFactor>(null!, null!);

            // assert
            Assert.Throws<ArgumentNullException>("context", action);
        }

        [Fact]
        public void ThrowsOnNullConfig()
        {
            // arrange
            var factory = new WeakActivationStateFactory();
            var context = new FakeGrainActivationContext();

            // act
            void action() => factory.Create<FakeWeakActivationFactor>(context, null!);

            // assert
            Assert.Throws<ArgumentNullException>("config", action);
        }

        [Fact]
        public void ThrowsOnNonRegisteredDefaultResourceGovernor()
        {
            // arrange
            var factory = new WeakActivationStateFactory();
            var context = new FakeGrainActivationContext()
            {
                GrainType = typeof(object),
                ActivationServices = new ServiceCollection()
                    .BuildServiceProvider()
            };
            var config = new FakeWeakActivationStateConfiguration();

            // act
            void action() => factory.Create<FakeWeakActivationFactor>(context, config);

            // assert
            var result = Assert.Throws<BadWeakActivationConfigException>(action);
            Assert.Equal(Resources.Exception_NoDefaultResourceGovernorFoundForGrainType_X.Format(typeof(object).FullName!), result.Message);
        }

        [Fact]
        public void ThrowsOnNonRegisteredNamedResourceGovernor()
        {
            // arrange
            var factory = new WeakActivationStateFactory();
            var context = new FakeGrainActivationContext()
            {
                GrainType = typeof(object),
                ActivationServices = new ServiceCollection()
                    .BuildServiceProvider()
            };
            var config = new FakeWeakActivationStateConfiguration
            {
                ResourceGovernorName = "FakeProvider"
            };

            // act
            void action() => factory.Create<FakeWeakActivationFactor>(context, config);

            // assert
            var result = Assert.Throws<BadWeakActivationConfigException>(action);
            Assert.Equal(Resources.Exception_NoResourceGovernorNamed_X_FoundForGrainType_X.Format("FakeProvider", typeof(object).FullName!), result.Message);
        }

        [Fact]
        public void CreatesDefaultState()
        {
            // arrange
            var factory = new WeakActivationStateFactory();
            var governor = new FakeResourceGovernor();
            var lifecycle = new FakeGrainLifecycle();
            var context = new FakeGrainActivationContext()
            {
                GrainType = typeof(object),
                ActivationServices = new ServiceCollection()
                    .AddSingleton<IResourceGovernor>(governor)
                    .BuildServiceProvider(),
                ObservableLifecycle = lifecycle
            };
            var config = new FakeWeakActivationStateConfiguration();

            // act
            var state = factory.Create<FakeWeakActivationFactor>(context, config);

            // assert
            Assert.NotNull(state);
            var (observerName, stage, observer) = Assert.Single(lifecycle.Subscriptions);
            Assert.Equal(typeof(WeakActivationState<FakeWeakActivationFactor>).FullName, observerName);
            Assert.Equal(GrainLifecycleStage.SetupState, stage);
            Assert.Same(state, observer);
        }

        [Fact]
        public void CreatesNamedState()
        {
            // arrange
            var factory = new WeakActivationStateFactory();
            var lifecycle = new FakeGrainLifecycle();
            var context = new FakeGrainActivationContext()
            {
                GrainType = typeof(object),
                ActivationServices = new ServiceCollection()
                    .AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>))
                    .AddSingletonNamedService<IResourceGovernor>("FakeGovernor", (sp, name) => new FakeResourceGovernor())
                    .BuildServiceProvider(),
                ObservableLifecycle = lifecycle
            };
            var config = new FakeWeakActivationStateConfiguration
            {
                ResourceGovernorName = "FakeGovernor"
            };

            // act
            var state = factory.Create<FakeWeakActivationFactor>(context, config);

            // assert
            Assert.NotNull(state);
            var (observerName, stage, observer) = Assert.Single(lifecycle.Subscriptions);
            Assert.Equal(typeof(WeakActivationState<FakeWeakActivationFactor>).FullName, observerName);
            Assert.Equal(GrainLifecycleStage.SetupState, stage);
            Assert.Same(state, observer);
        }
    }
}