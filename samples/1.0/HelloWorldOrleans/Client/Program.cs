using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        private static async Task Main()
        {
            var client = new ClientBuilder()
                .UseLocalhostClustering()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .AddOutkeep()
                .Build();

            var logger = client.ServiceProvider.GetService<ILogger<Program>>();

            logger.LogInformation("Connecting...");
            await client.Connect(async e =>
            {
                await Task.Delay(1000);

                logger.LogInformation("Retrying...");
                return true;
            });
            logger.LogInformation("Connected");

            var grain = client.GetCacheGrain(Guid.NewGuid().ToString());

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

            await client.Close();
            logger.LogInformation("Disconnected");
        }
    }
}