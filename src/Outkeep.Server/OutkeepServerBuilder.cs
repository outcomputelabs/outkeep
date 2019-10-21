using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace Outkeep.Server
{
    internal class OutkeepServerBuilder : IOutkeepServerBuilder
    {
        private readonly List<Action<HostBuilderContext, IServiceCollection>> configurators = new List<Action<HostBuilderContext, IServiceCollection>>();

        public IOutkeepServerBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            configurators.Add((context, services) => configure(services));

            return this;
        }

        public IOutkeepServerBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            configurators.Add(configure);

            return this;
        }

        public void Build(HostBuilderContext context, IServiceCollection services)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddHostedService<OutkeepServerHostedService>();

            foreach (var configure in configurators)
            {
                configure(context, services);
            }
        }
    }
}