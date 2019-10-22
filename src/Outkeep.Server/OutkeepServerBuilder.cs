using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using Outkeep.Implementations;
using System;
using System.Collections.Generic;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Outkeep.Server
{
    internal class OutkeepServerBuilder : IOutkeepServerBuilder
    {
        private readonly List<Action<HostBuilderContext, IServiceCollection>> servicesConfigurators = new List<Action<HostBuilderContext, IServiceCollection>>();
        private readonly List<Action<HostBuilderContext, ISiloBuilder>> siloConfigurators = new List<Action<HostBuilderContext, ISiloBuilder>>();

        public IOutkeepServerBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            servicesConfigurators.Add((context, services) => configure(services));

            return this;
        }

        public IOutkeepServerBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            servicesConfigurators.Add(configure);

            return this;
        }

        public IOutkeepServerBuilder ConfigureSilo(Action<HostBuilderContext, ISiloBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            siloConfigurators.Add(configure);

            return this;
        }

        public IOutkeepServerBuilder ConfigureSilo(Action<ISiloBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            siloConfigurators.Add((context, orleans) => configure(orleans));

            return this;
        }

        public void Build(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (services == null) throw new ArgumentNullException(nameof(services));

            services
                .AddHostedService<OutkeepServerHostedService>()
                .AddSingleton<DistributedCacheOptionsValidator>();

            foreach (var configure in siloConfigurators)
            {
                builder.UseOrleans(configure);
            }

            foreach (var configure in servicesConfigurators)
            {
                builder.ConfigureServices(configure);
            }
        }
    }
}