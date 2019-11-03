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