using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace Outkeep.Client
{
    internal class OutkeepClientBuilder : IOutkeepClientBuilder
    {
        private readonly List<Action<HostBuilderContext, IServiceCollection>> configurators = new List<Action<HostBuilderContext, IServiceCollection>>();

        public IOutkeepClientBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
        {
            configurators.Add(configure);

            return this;
        }

        public void Build(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<OutkeepClientHostedService>();

            foreach (var configure in configurators)
            {
                configure(context, services);
            }
        }
    }
}