using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Outkeep
{
    /// <summary>
    /// Offers a convenience integration point for optional extensions.
    /// </summary>
    public interface IOutkeepServerBuilder
    {
        /// <summary>
        /// Configures services upon the outkeep server instance.
        /// </summary>
        IOutkeepServerBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure);

        /// <summary>
        /// Configures the underlying Orleans silo that supports the outkeep server instance.
        /// </summary>
        IOutkeepServerBuilder ConfigureSilo(Action<HostBuilderContext, ISiloBuilder> configure);
    }
}