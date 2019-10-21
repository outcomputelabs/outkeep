using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Outkeep.Client
{
    public interface IOutkeepClientBuilder
    {
        IOutkeepClientBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure);

        IOutkeepClientBuilder ConfigureServices(Action<IServiceCollection> configure);
    }
}