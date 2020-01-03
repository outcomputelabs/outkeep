using Microsoft.Extensions.Hosting;
using System;

namespace Outkeep.Core.Caching
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="CacheDirector"/> to the <see cref="IHostBuilder"/> as an <see cref="ICacheDirector"/>.
        /// </summary>
        public static IHostBuilder UseCacheDirector(this IHostBuilder builder, Action<HostBuilderContext, CacheDirectorOptions> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices((context, services) =>
            {
                services.AddCacheDirector(options => configure(context, options));
            });
        }
    }
}