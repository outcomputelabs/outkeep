using Microsoft.Extensions.DependencyInjection;
using System;

namespace Outkeep.Core
{
    public static class OutkeepServerBuilderMemoryCacheStorageExtensions
    {
        public static IOutkeepServerBuilder AddMemoryCacheStorage(this IOutkeepServerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<ICacheStorage, MemoryCacheStorage>();
            });
        }
    }
}