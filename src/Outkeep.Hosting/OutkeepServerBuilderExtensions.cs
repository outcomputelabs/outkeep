using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;

namespace Outkeep.Hosting
{
    public static class OutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder Configure<TOptions>(this IOutkeepServerBuilder builder, Action<TOptions> configure) where TOptions : class
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services =>
            {
                services.Configure(configure);
            });
        }

        public static IOutkeepServerBuilder ConfigureServices(this IOutkeepServerBuilder builder, Action<IServiceCollection> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureServices((context, services) => configure(services));
        }

        public static IOutkeepServerBuilder ConfigureSilo(this IOutkeepServerBuilder builder, Action<ISiloBuilder> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureSilo((context, silo) => configure(silo));
        }
    }
}