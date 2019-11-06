using Orleans.Hosting;
using System;
using System.Net;

namespace Outkeep.Hosting
{
    public static class OutkeepHostingExtensions
    {
        public static IOutkeepServerBuilder UseStandaloneDefaults(this IOutkeepServerBuilder outkeep)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            return outkeep.UseStandaloneClustering();
        }

        public static IOutkeepServerBuilder UseStandaloneClustering(this IOutkeepServerBuilder outkeep,
            int siloStartPort = 11111,
            int siloEndPort = 11199,
            int gatewayStartPort = 30000,
            int gatewayEndPort = 30099,
            string serviceId = "dev",
            string clusterId = "dev")
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            outkeep.ConfigureSilo((context, silo) =>
            {
                silo.UseLocalhostClustering(
                    TcpHelper.GetFreePort(siloStartPort, siloEndPort),
                    TcpHelper.GetFreePort(gatewayStartPort, gatewayEndPort),
                    new IPEndPoint(IPAddress.Loopback, siloStartPort),
                    serviceId,
                    clusterId);
            });

            return outkeep;
        }
    }
}