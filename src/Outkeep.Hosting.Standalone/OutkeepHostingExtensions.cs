using Orleans.Hosting;
using System.Diagnostics.Contracts;
using System.Net;

namespace Outkeep.Hosting
{
    public static class OutkeepHostingExtensions
    {
        public static IOutkeepServerBuilder UseStandaloneClustering(this IOutkeepServerBuilder outkeep,
            int siloStartPort = 11111,
            int siloEndPort = 11199,
            int gatewayStartPort = 30000,
            int gatewayEndPort = 30099,
            string serviceId = "dev",
            string clusterId = "dev")
        {
            Contract.Requires(outkeep != null);

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