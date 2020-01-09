using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Outkeep.Core.Tests.Fakes
{
    public class FakeSiloBuilder : ISiloBuilder
    {
        private readonly List<Action<HostBuilderContext, IServiceCollection>> _configurators = new List<Action<HostBuilderContext, IServiceCollection>>();

        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        public ISiloBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configurators.Add(configureDelegate);
            return this;
        }

        public IServiceProvider BuildServiceProvider(HostBuilderContext context)
        {
            var services = new ServiceCollection();

            foreach (var configure in _configurators)
            {
                configure(context, services);
            }

            return services.BuildServiceProvider();
        }
    }
}