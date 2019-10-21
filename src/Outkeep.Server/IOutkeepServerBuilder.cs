using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Outkeep.Server
{
    public interface IOutkeepServerBuilder
    {
        IOutkeepServerBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure);

        IOutkeepServerBuilder ConfigureServices(Action<IServiceCollection> configure);
    }
}