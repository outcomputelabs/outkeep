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
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return outkeep.ConfigureServices(services =>
            {
                services.AddHostedService<RestApiHostedService>();
                services.AddOptions<RestApiServerOptions>()
                    .Configure(configure)
                    .ValidateDataAnnotations();
            });
        }
    }
}