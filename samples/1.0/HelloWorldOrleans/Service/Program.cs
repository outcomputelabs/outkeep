using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            using var host = Host
                .CreateDefaultBuilder()
                .UseOrleans(orleans =>
                {
                    orleans
                        .UseLocalhostClustering();

                    orleans
                        .AddOutkeep();
                })
                .UseConsoleLifetime()
                .Build();

            await host.StartAsync();

            var logger = host.Services.GetService<ILogger<Program>>();
            var factory = host.Services.GetService<IGrainFactory>();
            var grain = factory.GetCacheGrain(Guid.NewGuid().ToString());

            // cache a value
            var value = Guid.NewGuid().ToByteArray();
            logger.LogInformation("Setting value {Value}", value);
            await grain.SetAsync(value.AsImmutable(), null, null);

            // read the cached value
            var pulse = await grain.GetAsync();
            logger.LogInformation("Received pulse (Tag = {Tag}, Value = {Value})", pulse.Tag, pulse.Value);

            // wait for the next value
            var task = grain.PollAsync(pulse.Tag);
            logger.LogInformation("Waiting for next pulse...");

            // set the next value
            value = Guid.NewGuid().ToByteArray();
            logger.LogInformation("Setting next value {Value}", value);
            await grain.SetAsync(value.AsImmutable(), null, null);

            // verify the wait
            pulse = await task;
            logger.LogInformation("Received reactive pulse (Tag = {Tag}, Value = {Value})", pulse.Tag, pulse.Value);

            await host.WaitForShutdownAsync();
        }
    }
}