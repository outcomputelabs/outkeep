using Microsoft.Extensions.DependencyInjection;
using Outkeep.Core.Storage;
using System;

namespace Outkeep.Core
{
    public static class NullCacheStorageOutkeepServerBuilderExtensions
    {
        /// <summary>
        /// Adds a singleton <see cref="NullCacheStorage"/> to the <see cref="IOutkeepServerBuilder"/>.
        /// </summary>
        public static IOutkeepServerBuilder AddNullCacheStorage(this IOutkeepServerBuilder builder)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<ICacheStorage, NullCacheStorage>();
            });
        }
    }
}