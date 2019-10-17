using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Outkeep.Server
{
    public static class OutkeepHostBuilderExtensions
    {
        public static IHostBuilder UseOutkeepServer(this IHostBuilder builder, Action<OutkeepServerOptions> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.UseOutkeepServer((context, options) => configure(options));
        }

        public static IHostBuilder UseOutkeepServer(this IHostBuilder builder, Action<HostBuilderContext, OutkeepServerOptions> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureServices((context, services) =>
            {
                services.AddOutkeepServer();
                services.Configure<OutkeepServerOptions>(options => configure(context, options));
            });
        }
    }
}