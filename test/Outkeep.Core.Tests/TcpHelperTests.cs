using Moq;
using Outkeep.Core.Properties;
using Outkeep.Core.Tcp;
using System;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Outkeep.Core.Tests
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
            Assert.Throws<ArgumentOutOfRangeException>("start", () => helper.GetFreePort(0, IPEndPoint.MaxPort));
        }

        [Fact]
        public void RefusesHighEndPort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            var helper = new TcpHelper(factory);

            // act and assert
            Assert.Throws<ArgumentOutOfRangeException>("end", () => helper.GetFreePort(1, IPEndPoint.MaxPort + 1));
        }

        [Fact]
        public void RefusesOverlappedEndPort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            var helper = new TcpHelper(factory);

            // act and assert
            Assert.Throws<ArgumentOutOfRangeException>("end", () => helper.GetFreePort(IPEndPoint.MaxPort, IPEndPoint.MaxPort - 1));
        }

        [Fact]
        public void GetsFirstFreePort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            Mock.Get(factory).Setup(x => x.Create(1, true).Start()).Throws(new SocketException());
            Mock.Get(factory).Setup(x => x.Create(2, true).Start()).Throws(new SocketException());
            Mock.Get(factory).Setup(x => x.Create(3, true).Stop()).Throws(new SocketException());

            var helper = new TcpHelper(factory);

            // act
            var port = helper.GetFreePort(1, 1 << 16 - 1);

            // assert
            Assert.Equal(3, port);
        }

        [Fact]
        public void GetsFirstFreePortWithSingleParameter()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            Mock.Get(factory).Setup(x => x.Create(1, true).Start()).Throws(new SocketException());
            Mock.Get(factory).Setup(x => x.Create(2, true).Start()).Throws(new SocketException());
            Mock.Get(factory).Setup(x => x.Create(3, true).Stop()).Throws(new SocketException());

            var helper = new TcpHelper(factory);

            // act
            var port = helper.GetFreePort(1);

            // assert
            Assert.Equal(3, port);
        }

        [Fact]
        public void ThrowsOnNoFreePort()
        {
            // arrange
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            Mock.Get(factory).Setup(x => x.Create(1, true).Start()).Throws(new SocketException());
            Mock.Get(factory).Setup(x => x.Create(2, true).Start()).Throws(new SocketException());
            Mock.Get(factory).Setup(x => x.Create(3, true).Start()).Throws(new SocketException());

            var helper = new TcpHelper(factory);

            // act and assert
            var error = Assert.Throws<InvalidOperationException>(() => helper.GetFreePort(1, 3));
            Assert.Equal(Resources.Exception_ThereAreNoFreePortsWithinTheInputRange, error.Message);
        }

        [Fact]
        public void GetsFreeDynamicPort()
        {
            // arrange
            var port = 51234;
            var factory = Mock.Of<ITcpListenerWrapperFactory>(x =>
                x.Create(0, true).LocalEndpoint == new IPEndPoint(IPAddress.Loopback, port));
            var helper = new TcpHelper(factory);

            // act
            var result = helper.GetFreeDynamicPort();

            // assert
            Assert.Equal(port, result);
        }

        [Fact]
        public void GetFreeDynamicPortThrowsOnUnexpectedEndPointType()
        {
            // arrange
            var port = 51234;
            var factory = Mock.Of<ITcpListenerWrapperFactory>(x =>
                x.Create(0, true).LocalEndpoint == new DnsEndPoint("localhost", port));
            var helper = new TcpHelper(factory);

            // act and assert
            var error = Assert.Throws<InvalidOperationException>(() => helper.GetFreeDynamicPort());
            Assert.Equal(Resources.Exception_UnexpectedEndpointType, error.Message);
        }

        [Fact]
        public void GetFreeDynamicPortThrowsOnSocketError()
        {
            // arrange
            var ex = new SocketException();
            var factory = Mock.Of<ITcpListenerWrapperFactory>();
            Mock.Get(factory).Setup(x => x.Create(0, true).Start()).Throws(ex);
            var helper = new TcpHelper(factory);

            // act and assert
            var error = Assert.Throws<InvalidOperationException>(() => helper.GetFreeDynamicPort());
            Assert.Equal(Resources.Exception_CouldNotOpenADynamicPortForExclusiveUse, error.Message);
            Assert.Same(ex, error.InnerException);
        }

        [Fact]
        public void HasDefault()
        {
            Assert.NotNull(TcpHelper.Default);
        }
    }
}