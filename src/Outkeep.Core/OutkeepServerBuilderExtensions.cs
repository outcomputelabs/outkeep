using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans.Hosting;
using Outkeep.Core.Storage;
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

        /// <summary>
        /// Adds a singleton <see cref="MemoryCacheStorage"/> to the <see cref="IOutkeepServerBuilder"/>.
        /// </summary>
        public static IOutkeepServerBuilder AddMemoryCacheStorage(this IOutkeepServerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<ICacheStorage, MemoryCacheStorage>();
            });
        }

        /// <summary>
        /// Adds a singleton <see cref="MemoryCacheStorage"/> to the <see cref="IOutkeepServerBuilder"/> if an <see cref="ICacheStorage"/> implementation is not yet present.
        /// </summary>
        public static IOutkeepServerBuilder TryAddMemoryCacheStorage(this IOutkeepServerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices((context, services) =>
            {
                services.TryAddSingleton<ICacheStorage, MemoryCacheStorage>();
            });
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