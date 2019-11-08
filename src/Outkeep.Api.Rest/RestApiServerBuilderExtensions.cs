using Microsoft.Extensions.DependencyInjection;
using Outkeep.Api.Rest;
using System;

namespace Outkeep.Hosting
{
    public static class RestApiServerBuilderExtensions
    {
        public static IOutkeepServerBuilder UseRestApi(this IOutkeepServerBuilder outkeep, Action<RestApiServerOptions> configure)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            return outkeep.ConfigureServices(services =>
            {
                services.AddHostedService<RestApiHostedService>();
                services.Configure(configure);
            });
        }
    }
}