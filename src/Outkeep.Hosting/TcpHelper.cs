using Outkeep.Hosting.Properties;
using System;
using System.Net;
using System.Net.Sockets;

namespace Outkeep.Hosting
{
    public static class TcpHelper
    {
        public static int GetFreePort(int start, int end)
        {
            if (start < 1) throw new ArgumentOutOfRangeException(nameof(start));
            if (end >= (1 << 16)) throw new ArgumentOutOfRangeException(nameof(end));
            if (end < start) throw new ArgumentOutOfRangeException(nameof(end));

            for (var port = start; port <= end; ++port)
            {
                var listener = TcpListener.Create(port);
                try
                {
                    listener.ExclusiveAddressUse = true;
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

        public static int GetFreeDynamicPort()
        {
            var listener = TcpListener.Create(0);
            listener.ExclusiveAddressUse = true;

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
    }
}