using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;

namespace Outkeep.Core
{
    public static class OutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder ConfigureSilo(this IOutkeepServerBuilder builder, Action<ISiloBuilder> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureSilo((context, silo) => configure(silo));
        }

        public static IOutkeepServerBuilder AddCoreServices(this IOutkeepServerBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddCoreServices();
            });
        }
    }
}