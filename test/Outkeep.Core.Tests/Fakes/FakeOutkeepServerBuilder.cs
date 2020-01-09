using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Outkeep.Core.Tests.Fakes
{
    public class FakeOutkeepServerBuilder : IOutkeepServerBuilder
    {
        private readonly List<Action<HostBuilderContext, IServiceCollection>> _serviceConfigurators = new List<Action<HostBuilderContext, IServiceCollection>>();
        private readonly List<Action<HostBuilderContext, ISiloBuilder>> _siloConfigurators = new List<Action<HostBuilderContext, ISiloBuilder>>();

        public IOutkeepServerBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
        {
            _serviceConfigurators.Add(configure);
            return this;
        }

        public IOutkeepServerBuilder ConfigureSilo(Action<HostBuilderContext, ISiloBuilder> configure)
        {
            _siloConfigurators.Add(configure);
            return this;
        }

        public IServiceProvider BuildServiceProvider(HostBuilderContext context, ISiloBuilder silo)
        {
            var services = new ServiceCollection();

            foreach (var configure in _serviceConfigurators)
            {
                configure(context, services);
            }

            foreach (var configure in _siloConfigurators)
            {
                configure(context, silo);
            }

            return services.BuildServiceProvider();
        }
    }
}