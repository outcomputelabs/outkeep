using Microsoft.Extensions.DependencyInjection;
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
    }
}