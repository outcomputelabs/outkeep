using Orleans.Hosting;
using System;

namespace Outkeep.Hosting
{
    public static class OutkeepHostingExtensions
    {
        public static IOutkeepServerBuilder UseStandaloneDefaults(this IOutkeepServerBuilder outkeep)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            return outkeep.UseStandaloneClustering();
        }

        public static IOutkeepServerBuilder UseStandaloneClustering(this IOutkeepServerBuilder outkeep)
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