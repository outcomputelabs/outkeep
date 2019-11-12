using Microsoft.Extensions.DependencyInjection;
using Outkeep.Api.Http;
using System;

namespace Outkeep.Hosting
{
    public static class RestApiServerBuilderExtensions
    {
        public static IOutkeepServerBuilder UseRestApi(this IOutkeepServerBuilder outkeep, Action<RestApiServerOptions> configure)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            return outkeep.ConfigureServices((context, services) =>
            {
                services.AddHostedService<RestApiHostedService>();
                services.AddOptions<RestApiServerOptions>()
                    .Configure(configure)
                    .ValidateDataAnnotations();
            });
        }
    }
}