using Outkeep.Server;
using System;

namespace Microsoft.Extensions.Hosting
{
    public static class OutkeepHostBuilderExtensions
    {
        private const string HostBuilderContextKey = nameof(OutkeepServerBuilder);

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
                if (context.Properties.TryGetValue(HostBuilderContextKey, out var existing))
                {
                    outkeep = (OutkeepServerBuilder)existing;
                }
                else
                {
                    outkeep = new OutkeepServerBuilder();
                    context.Properties[HostBuilderContextKey] = outkeep;
                }

                configure(context, outkeep);

                outkeep.Build(context, services);
            });
        }
    }
}