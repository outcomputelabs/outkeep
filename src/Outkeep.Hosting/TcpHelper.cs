using Outkeep.Hosting.Properties;
using System;
using System.Net;
using System.Net.Sockets;

namespace Outkeep.Hosting
{
    public sealed class TcpHelper
    {
        private readonly ITcpListenerWrapperFactory _factory;

        public TcpHelper(ITcpListenerWrapperFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public int GetFreePort(int start, int end)
        {
            if (start < 1) throw new ArgumentOutOfRangeException(nameof(start));
            if (end >= (1 << 16)) throw new ArgumentOutOfRangeException(nameof(end));
            if (end < start) throw new ArgumentOutOfRangeException(nameof(end));

            for (var port = start; port <= end; ++port)
            {
                var listener = _factory.Create(port, true);
                try
                {
                    listener.Start();
                    return ((IPEndPoint)listener.LocalEndpoint).Port;
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

            throw new InvalidOperationException(Resources.ThereAreNoFreePortsWithinTheInputRange);
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
                    throw new InvalidOperationException(Resources.ExceptionUnexpectedEndpointType);
                }
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException(Resources.ExceptionCouldNotOpenADynamicPortForExclusiveUse, ex);
            }
            finally
            {
                listener.Stop();
            }
        }

        public static readonly TcpHelper Default = new TcpHelper(TcpListenerWrapperFactory.Default);
    }
}