using Outkeep.Hosting.Properties;
using System;
using System.Net;
using System.Net.Sockets;

namespace Outkeep.Hosting
{
    public static class TcpHelper
    {
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
                    throw new InvalidOperationException(Resources.Exception_UnexpectedEndpointType);
                }
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException(Resources.Exception_CouldNotFindAFreeDynamicPort, ex);
            }
            finally
            {
                listener.Stop();
            }
        }

        public static bool TryGetFreePort(int first, int last, out int port)
        {
            if (first < 1) throw new ArgumentOutOfRangeException(nameof(first));
            if (last < first) throw new ArgumentOutOfRangeException(nameof(last));
            if (last > 1 << 16) throw new ArgumentOutOfRangeException(nameof(last));

            for (port = first; port <= last; ++port)
            {
                var listener = TcpListener.Create(port);
                listener.ExclusiveAddressUse = true;

                try
                {
                    listener.Start();
                }
                catch (SocketException)
                {
                    continue;
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

                return true;
            }

            return false;
        }
    }
}