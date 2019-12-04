using System.Net;
using System.Net.Sockets;

namespace Outkeep.Hosting
{
    /// <summary>
    /// Wraps static calls to the <see cref="TcpListener"/> class to facilitate testing.
    /// </summary>
    public class TcpListenerWrapper : ITcpListenerWrapper
    {
        private readonly TcpListener _listener;

        public TcpListenerWrapper(int port, bool exclusiveAddressUse)
        {
            _listener = TcpListener.Create(port);
            _listener.ExclusiveAddressUse = exclusiveAddressUse;
        }

        public EndPoint LocalEndpoint => _listener.LocalEndpoint;

        public void Start() => _listener.Start();

        public void Stop() => _listener.Stop();
    }
}