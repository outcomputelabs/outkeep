using System;
using System.Net.Sockets;

namespace Outkeep.Server
{
    public static class TcpHelper
    {
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