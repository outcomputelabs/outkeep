using Outkeep.Governance;
using System;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class WeakActivationStateAttributeMapperTests
    {
        [Fact]
        public void GetFactoryThrowsOnNullParameter()
        {
            var mapper = new WeakActivationStateAttributeMapper();

            Assert.Throws<ArgumentNullException>("parameter", () => mapper.GetFactory(null!, null!));
        }
    }
}