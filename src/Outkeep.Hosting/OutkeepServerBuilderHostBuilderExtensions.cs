﻿using Microsoft.Extensions.DependencyInjection;
using Outkeep.Hosting;
using System;

namespace Microsoft.Extensions.Hosting
{
    public static class OutkeepServerBuilderHostBuilderExtensions
    {
        private const string HostBuilderContextKey = nameof(OutkeepServerBuilder);

        public static IHostBuilder UseOutkeepServer(this IHostBuilder builder, Action<IOutkeepServerBuilder> configure)
        {
            return builder.UseOutkeepServer((context, outkeep) => configure(outkeep));
        }

        public static IHostBuilder UseOutkeepServer(this IHostBuilder builder, Action<HostBuilderContext, IOutkeepServerBuilder> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            OutkeepServerBuilder outkeep;
            if (builder.Properties.TryGetValue(HostBuilderContextKey, out var current))
            {
                outkeep = (OutkeepServerBuilder)current;
            }
            else
            {
                builder.Properties[HostBuilderContextKey] = outkeep = new OutkeepServerBuilder(builder);

                // wire up services configuration at build time
                builder.ConfigureServices((context, services) =>
                {
                    outkeep.Build(context, services);
                });
            }

            return builder.ConfigureServices((context, services) =>
            {
                configure(context, outkeep);
            });
        }
    }
}