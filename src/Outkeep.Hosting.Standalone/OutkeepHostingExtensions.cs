using Orleans.Hosting;
using Outkeep.Core.Tcp;
using System;
using System.Net;

namespace Outkeep.Core
{
    public static class OutkeepHostingExtensions
    {
        public static IOutkeepServerBuilder UseStandaloneClustering(
            this IOutkeepServerBuilder outkeep,
            int siloPort = 11111,
            int gatewayPort = 30000,
            string serviceId = "dev",
            string clusterId = "dev",
            bool searchPorts = false)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            outkeep.ConfigureSilo((context, silo) =>
            {
                var actualSiloPort = searchPorts ? TcpHelper.Default.GetFreePort(siloPort) : siloPort;
                var actualGatewayPort = searchPorts ? TcpHelper.Default.GetFreePort(gatewayPort) : gatewayPort;

                silo.UseLocalhostClustering(actualSiloPort, actualGatewayPort, new IPEndPoint(IPAddress.Loopback, siloPort), serviceId, clusterId);
            });

            return outkeep;
        }
    }
}