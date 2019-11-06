using System;

namespace Outkeep.Hosting
{
    public static class OutkeepHostingExtensions
    {
        public static IOutkeepServerBuilder UseStandaloneDefaults(this IOutkeepServerBuilder outkeep)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            return outkeep;
        }
    }
}