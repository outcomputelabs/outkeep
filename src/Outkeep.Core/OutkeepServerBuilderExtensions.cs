using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Outkeep.Core
{
    /// <summary>
    /// Quality-of-life extensions for <see cref="IOutkeepServerBuilder"/>.
    /// </summary>
    public static class OutkeepServerBuilderExtensions
    {
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
    }
}