using Moq;
using Outkeep.Governance;
using System;
using System.Reflection;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class WeakActivationStateAttributeMapperTests
    {
        [Fact]
        public void GetFactoryThrowsOnNullParameter()
        {
            var mapper = new WeakActivationStateAttributeMapper();

            Assert.Throws<ArgumentNullException>("parameter", () => mapper.GetFactory(null!, null!));
        }

        [Fact]
        public void GetFactoryThrowsOnNullMetadata()
        {
            var mapper = new WeakActivationStateAttributeMapper();
            var parameter = Mock.Of<ParameterInfo>();

            Assert.Throws<ArgumentNullException>("metadata", () => mapper.GetFactory(parameter, null!));
        }
    }
}