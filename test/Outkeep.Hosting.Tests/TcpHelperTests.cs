using Moq;
using System;
using Xunit;

namespace Outkeep.Hosting.Tests
{
    public class TcpHelperTests
    {
        [Fact]
        public void RefusesLowStartPort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            var helper = new TcpHelper(factory);

            // act and assert
            Assert.Throws<ArgumentOutOfRangeException>("start", () => helper.GetFreePort(0, 60000));
        }

        [Fact]
        public void RefusesHighEndPort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            var helper = new TcpHelper(factory);

            // act and assert
            Assert.Throws<ArgumentOutOfRangeException>("end", () => helper.GetFreePort(1, 1 << 16 + 1));
        }

        [Fact]
        public void RefusesOverlappedEndPort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            var helper = new TcpHelper(factory);

            // act and assert
            Assert.Throws<ArgumentOutOfRangeException>("end", () => helper.GetFreePort(1 << 16 + 1, 1 << 16));
        }
    }
}