using Outkeep.Governance;
using Outkeep.Grains.Tests.Fakes;
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
    }
}