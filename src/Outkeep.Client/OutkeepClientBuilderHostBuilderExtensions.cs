using Outkeep.Client;
using System;

namespace Microsoft.Extensions.Hosting
{
    public static class OutkeepClientBuilderHostBuilderExtensions
    {
        private const string HostBuilderContextKey = nameof(OutkeepClientBuilder);

        public static IHostBuilder UseOutkeepClient(this IHostBuilder builder, Action<IOutkeepClientBuilder> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.UseOutkeepClient((context, outkeep) => configure(outkeep));
        }

        public static IHostBuilder UseOutkeepClient(this IHostBuilder builder, Action<HostBuilderContext, IOutkeepClientBuilder> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureServices((context, services) =>
            {
                OutkeepClientBuilder outkeep;
                if (context.Properties.TryGetValue(HostBuilderContextKey, out var existing))
                {
                    outkeep = (OutkeepClientBuilder)existing;
                }
                else
                {
                    outkeep = new OutkeepClientBuilder();
                    context.Properties[HostBuilderContextKey] = outkeep;
                }

                configure(context, outkeep);

                outkeep.Build(context, services);
            });
        }
    }
}