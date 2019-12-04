using Moq;
using System;
using Xunit;

namespace Outkeep.Hosting.Tests
{
    public class TcpHelperTests
    {
        [Fact]
        public void RefusesLowPort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            var helper = new TcpHelper(factory);

            // act and assert
            Assert.Throws<ArgumentOutOfRangeException>("start", () => helper.GetFreePort(0, 60000));
        }
    }
}