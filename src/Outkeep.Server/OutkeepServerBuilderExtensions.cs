using Orleans.Hosting;
using System;

namespace Outkeep.Server
{
    public static class OutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder UseLocalhostSetup(this IOutkeepServerBuilder outkeep)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            outkeep.ConfigureSilo(silo =>
            {
                silo.UseLocalhostClustering();
            });

            return outkeep;
        }
    }
}