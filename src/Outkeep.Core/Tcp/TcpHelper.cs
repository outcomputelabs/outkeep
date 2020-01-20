using Outkeep.Core.Properties;
using System;
using System.Net;
using System.Net.Sockets;

namespace Outkeep.Core.Tcp
{
    public sealed class TcpHelper
    {
        private readonly ITcpListenerWrapperFactory _factory;

        public TcpHelper(ITcpListenerWrapperFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Attempts to find a free port starting from the one given.
        /// </summary>
        public int GetFreePort(int start) => GetFreePort(start, IPEndPoint.MaxPort);

        /// <summary>
        /// Attempts to find a free port between the range given.
        /// </summary>
        public int GetFreePort(int start, int end)
        {
            if (start < 1) throw new ArgumentOutOfRangeException(nameof(start));
            if (start > IPEndPoint.MaxPort) throw new ArgumentOutOfRangeException(nameof(start));
            if (end < start) throw new ArgumentOutOfRangeException(nameof(end));
            if (end > IPEndPoint.MaxPort) throw new ArgumentOutOfRangeException(nameof(end));

            for (var port = start; port <= end; ++port)
            {
                var listener = _factory.Create(port, true);
                try
                {
                    listener.Start();
                    return port;
                }
                catch (SocketException)
                {
                    // noop
                }
                finally
                {
                    try
                    {
                        listener.Stop();
                    }
                    catch (SocketException)
                    {
                        // noop
                    }
                }
            }

            throw new InvalidOperationException(Resources.Exception_ThereAreNoFreePortsWithinTheInputRange);
        }

        public int GetFreeDynamicPort()
        {
            var listener = _factory.Create(0, true);

            try
            {
                listener.Start();
                if (listener.LocalEndpoint is IPEndPoint ip)
                {
                    return ip.Port;
                }
                else
                {
                    throw new InvalidOperationException(Resources.Exception_UnexpectedEndpointType);
                }
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException(Resources.Exception_CouldNotOpenADynamicPortForExclusiveUse, ex);
            }
            finally
            {
                listener.Stop();
            }
        }

        public static readonly TcpHelper Default = new TcpHelper(TcpListenerWrapperFactory.Default);
    }
}