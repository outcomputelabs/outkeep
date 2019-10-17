using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Outkeep.Client
{
    public static class OutkeepHostBuilderExtensions
    {
        public static IHostBuilder UseOutkeepClient(this IHostBuilder builder, Action<OutkeepClientOptions> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.UseOutkeepClient((context, options) => configure(options));
        }

        public static IHostBuilder UseOutkeepClient(this IHostBuilder builder, Action<HostBuilderContext, OutkeepClientOptions> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureServices((context, services) =>
            {
                services
                    .AddOutkeepClient()
                    .Configure<OutkeepClientOptions>(options => configure(context, options));
            });
        }
    }
}