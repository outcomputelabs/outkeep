using Microsoft.Extensions.DependencyInjection;
using Outkeep.Api.Http;
using System;

namespace Outkeep.Hosting
{
    public static class OutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder UseRestApi(this IOutkeepServerBuilder outkeep, Action<OutkeepHttpApiServerOptions> configure)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            return outkeep.ConfigureServices((context, services) =>
            {
                services.AddHostedService<OutkeepHttpApiHostedService>();
                services.AddOptions<OutkeepHttpApiServerOptions>()
                    .Configure(configure)
                    .ValidateDataAnnotations();
            });
        }
    }
}