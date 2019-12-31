using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Outkeep.Core.Caching
{
    public static class OutkeepServerBuilderExtensions
    {
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