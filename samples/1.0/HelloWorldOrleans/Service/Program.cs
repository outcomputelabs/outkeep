using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Concurrency;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;

namespace Service
{
    internal class Program
    {
        private static async Task Main()
        {
            var host = Host
                .CreateDefaultBuilder()
                .UseOrleans(orleans =>
                {
                    orleans.UseLocalhostClustering();
                })
                .UseOutkeepServer()
                .UseConsoleLifetime()
                .Build();

            await host.StartAsync();

            var factory = host.Services.GetService<IGrainFactory>();
            var grain = factory.GetCacheGrain(Guid.NewGuid().ToString());
            await grain.SetAsync(Guid.NewGuid().ToByteArray().AsImmutable(), null, null);

            await host.WaitForShutdownAsync();
        }
    }
}