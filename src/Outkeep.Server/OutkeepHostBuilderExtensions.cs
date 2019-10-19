using Microsoft.Extensions.Hosting;
using System;

namespace Outkeep.Server
{
    public static class OutkeepHostBuilderExtensions
    {
        public static IHostBuilder UseOutkeepServer(this IHostBuilder builder, Action<IOutkeepServerBuilder> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.UseOutkeepServer((context, outkeep) => configure(outkeep));
        }

        public static IHostBuilder UseOutkeepServer(this IHostBuilder builder, Action<HostBuilderContext, IOutkeepServerBuilder> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureServices((context, services) =>
            {
                OutkeepServerBuilder outkeep;
                if (context.Properties.TryGetValue(nameof(OutkeepServerBuilder), out var existing))
                {
                    outkeep = (OutkeepServerBuilder)existing;
                }
                else
                {
                    outkeep = new OutkeepServerBuilder(builder);
                    context.Properties[nameof(OutkeepServerBuilder)] = outkeep;
                }

                configure(context, outkeep);
            });
        }
    }
}