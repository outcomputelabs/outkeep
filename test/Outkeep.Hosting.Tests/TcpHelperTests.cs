using Moq;
using System;
using System.Net.Sockets;
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
            Assert.Throws<ArgumentOutOfRangeException>("start", () => helper.GetFreePort(0, 1 << 16 - 1));
        }

        [Fact]
        public void RefusesHighEndPort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            var helper = new TcpHelper(factory);

            // act and assert
            Assert.Throws<ArgumentOutOfRangeException>("end", () => helper.GetFreePort(1, 1 << 16));
        }

        [Fact]
        public void RefusesOverlappedEndPort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            var helper = new TcpHelper(factory);

            // act and assert
            Assert.Throws<ArgumentOutOfRangeException>("end", () => helper.GetFreePort(1 << 16, 1 << 16 - 1));
        }

        [Fact]
        public void GetsFirstFreePort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            Mock.Get(factory).Setup(x => x.Create(1, true).Start()).Throws(new SocketException());
            Mock.Get(factory).Setup(x => x.Create(2, true).Start()).Throws(new SocketException());
            Mock.Get(factory).Setup(x => x.Create(3, true)).Returns(Mock.Of<ITcpListenerWrapper>());

            var helper = new TcpHelper(factory);

            // act
            var port = helper.GetFreePort(1, 1 << 16 - 1);

            // assert
            Assert.Equal(3, port);
        }
    }
}