using Microsoft.Extensions.DependencyInjection;
using Outkeep.Api.Http;
using System;

namespace Outkeep.Hosting
{
    /// <summary>
    /// Extension methods for adding an <see cref="OutkeepHttpApiHostedService"/> to an <see cref="IOutkeepServerBuilder"/>.
    /// </summary>
    public static class OutkeepServerBuilderHttpApiExtensions
    {
        /// <summary>
        /// Sets up an Outkeep HTTP API on this <see cref="IOutkeepServerBuilder"/> instance.
        /// Successive calls to this extension method are cumulative and will only create a single API instance.
        /// </summary>
        public static IOutkeepServerBuilder UseHttpApi(this IOutkeepServerBuilder outkeep, Action<OutkeepHttpApiServerOptions> configure)
        {
            if (outkeep == null) throw new ArgumentNullException(nameof(outkeep));

            return outkeep.ConfigureServices((context, services) =>
            {
                services.TryAddHostedService<OutkeepHttpApiHostedService>();
                services.AddOptions<OutkeepHttpApiServerOptions>()
                    .Configure(configure)
                    .ValidateDataAnnotations();
            });
        }
    }
}