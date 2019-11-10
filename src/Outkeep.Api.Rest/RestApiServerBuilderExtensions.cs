using Microsoft.Extensions.DependencyInjection;
using Outkeep.Api.Rest;
using System;

namespace Outkeep.Hosting
{
    public static class RestApiServerBuilderExtensions
    {
        public static IOutkeepServerBuilder UseRestApi(this IOutkeepServerBuilder outkeep, Action<RestApiServerOptions> configure)
        {
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