﻿using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using System;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Outkeep.Hosting
{
    public interface IOutkeepServerBuilder
    {
        IOutkeepServerBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure);

        IOutkeepServerBuilder ConfigureSilo(Action<HostBuilderContext, ISiloBuilder> configure);
    }
}