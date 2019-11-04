using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Outkeep.Hosting
{
    internal class OutkeepServerBuilder : IOutkeepServerBuilder
    {
        private readonly IHostBuilder builder;

        public OutkeepServerBuilder(IHostBuilder builder)
        {
            this.builder = builder;
        }

        public IOutkeepServerBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            builder.ConfigureServices(configure);

            return this;
        }

        public IOutkeepServerBuilder ConfigureSilo(Action<HostBuilderContext, ISiloBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            builder.UseOrleans(configure);

            return this;
        }
    }
}