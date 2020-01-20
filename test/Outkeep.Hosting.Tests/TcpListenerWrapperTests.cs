using Outkeep.Core.Tcp;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Outkeep.Hosting.Tests
{
    public class TcpListenerWrapperTests
    {
        [Fact]
        public void Cycles()
        {
            // arrange
            var listener = TcpListener.Create(0);
            listener.ExclusiveAddressUse = true;
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            // act
            var wrapper = new TcpListenerWrapper(port, true);

            // assert
            var endpoint = Assert.IsType<IPEndPoint>(wrapper.LocalEndpoint);
            Assert.Equal(port, endpoint.Port);

            // act
            wrapper.Start();
            wrapper.Stop();
        }
    }
}