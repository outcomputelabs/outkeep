using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Outkeep.Core
{
    /// <summary>
    /// Quality-of-life extensions for <see cref="IOutkeepServerBuilder"/>.
    /// </summary>
    public static class OutkeepServerBuilderExtensions
    {
        /// <summary>
        /// Configures services upon the outkeep server instance.
        /// </summary>
        public static IOutkeepServerBuilder ConfigureServices(this IOutkeepServerBuilder builder, Action<IServiceCollection> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureServices((context, services) => configure(services));
        }

        /// <summary>
        /// Configures the underlying Orleans silo that supports the outkeep server instance.
        /// </summary>
        public static IOutkeepServerBuilder ConfigureSilo(this IOutkeepServerBuilder builder, Action<ISiloBuilder> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureSilo((context, silo) => configure(silo));
        }

        /// <inheritdoc cref="OptionsServiceCollectionExtensions.Configure{TOptions}(IServiceCollection, Action{TOptions})"/>
        public static IOutkeepServerBuilder Configure<TOptions>(this IOutkeepServerBuilder builder, Action<TOptions> configure) where TOptions : class
        {
            return builder.Configure<TOptions>((context, options) => configure(options));
        }

        /// <inheritdoc cref="OptionsServiceCollectionExtensions.Configure{TOptions}(IServiceCollection, Action{TOptions})"/>
        public static IOutkeepServerBuilder Configure<TOptions>(this IOutkeepServerBuilder builder, Action<HostBuilderContext, TOptions> configure) where TOptions : class
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices((context, services) =>
            {
                services.Configure<TOptions>(options => configure(context, options));
            });
        }
    }
}