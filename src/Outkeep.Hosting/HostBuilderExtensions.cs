﻿using Outkeep;
using Outkeep.Hosting;
using System;
using System.Diagnostics.Contracts;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        private const string HostBuilderContextKey = nameof(OutkeepServerBuilder);

        public static IHostBuilder UseOutkeepServer(this IHostBuilder builder, Action<IOutkeepServerBuilder> configure)
        {
            return builder.UseOutkeepServer((context, outkeep) => configure(outkeep));
        }

        public static IHostBuilder UseOutkeepServer(this IHostBuilder builder, Action<HostBuilderContext, IOutkeepServerBuilder> configure)
        {
            Contract.Requires(builder != null);

            OutkeepServerBuilder outkeep;
            if (builder.Properties.TryGetValue(HostBuilderContextKey, out var current))
            {
                outkeep = (OutkeepServerBuilder)current;
            }
            else
            {
                builder.Properties[HostBuilderContextKey] = outkeep = new OutkeepServerBuilder(builder);
            }

            outkeep.ConfigureOutkeep(configure);

            return builder;
        }
    }
}