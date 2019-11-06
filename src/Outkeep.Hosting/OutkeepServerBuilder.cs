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
        private readonly List<Action<HostBuilderContext, IOutkeepServerBuilder>> outkeepConfigurators = new List<Action<HostBuilderContext, IOutkeepServerBuilder>>();
        private readonly List<Action<HostBuilderContext, IServiceCollection>> serviceConfigurators = new List<Action<HostBuilderContext, IServiceCollection>>();
        private readonly List<Action<HostBuilderContext, ISiloBuilder>> siloConfigurators = new List<Action<HostBuilderContext, ISiloBuilder>>();

        public OutkeepServerBuilder(IHostBuilder builder)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public IOutkeepServerBuilder ConfigureOutkeep(Action<HostBuilderContext, IOutkeepServerBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            outkeepConfigurators.Add(configure);

            return this;
        }

        public IOutkeepServerBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            serviceConfigurators.Add(configure);

            return this;
        }

        public IOutkeepServerBuilder ConfigureSilo(Action<HostBuilderContext, ISiloBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            builder.UseOrleans(configure);

            return this;
        }

        public void Build(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<OutkeepServerHostedService>();

            foreach (var configure in outkeepConfigurators)
            {
                configure(context, this);
            }

            foreach (var configure in serviceConfigurators)
            {
                configure(context, services);
            }
        }
    }
}