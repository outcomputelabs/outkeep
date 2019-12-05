using Microsoft.Extensions.DependencyInjection;
using System;

namespace Outkeep.Client
{
    public static class OutkeepClientBuilderExtensions
    {
        public static IOutkeepClientBuilder ConfigureServices(this IOutkeepClientBuilder builder, Action<IServiceCollection> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices((context, services) => configure(services));
        }
    }
}