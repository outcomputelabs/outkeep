using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;
using System.Diagnostics.Contracts;

namespace Outkeep.Hosting
{
    public static class OutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder Configure<TOptions>(this IOutkeepServerBuilder builder, Action<TOptions> configure) where TOptions : class
        {
            return builder.ConfigureServices(services =>
            {
                services.Configure(configure);
            });
        }

        public static IOutkeepServerBuilder ConfigureServices(this IOutkeepServerBuilder builder, Action<IServiceCollection> configure)
        {
            Contract.Requires(builder != null);

            return builder.ConfigureServices((context, services) => configure(services));
        }

        public static IOutkeepServerBuilder ConfigureSilo(this IOutkeepServerBuilder builder, Action<ISiloBuilder> configure)
        {
            Contract.Requires(builder != null);

            return builder.ConfigureSilo((context, silo) => configure(silo));
        }
    }
}