using Microsoft.Extensions.DependencyInjection;
using Outkeep.Api.Http;
using System;
using System.Diagnostics.Contracts;

namespace Outkeep.Hosting
{
    public static class RestApiServerBuilderExtensions
    {
        public static IOutkeepServerBuilder UseRestApi(this IOutkeepServerBuilder outkeep, Action<RestApiServerOptions> configure)
        {
            Contract.Requires(outkeep != null);

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