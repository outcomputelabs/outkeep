using Orleans.Hosting;
using Outkeep.Api.Rest;
using System;

namespace Outkeep.Hosting
{
    public static class OutkeepRestApiServerBuilderExtensions
    {
        public static IOutkeepServerBuilder UseRestApi(this IOutkeepServerBuilder outkeep)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            return outkeep.ConfigureSilo(silo =>
            {
                silo.AddStartupTask<RestApiStartupTask>();
            });
        }
    }
}