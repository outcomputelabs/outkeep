using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Outkeep.Grains;
using Outkeep.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Outkeep.Application.Azure
{
    internal sealed class Program
    {
        private Program()
        {
        }

        private static Task Main()
        {
            return new HostBuilder()
                .ConfigureHostConfiguration(config =>
                {
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    config
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables(nameof(Outkeep))
                        .AddUserSecrets<Program>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddSerilog(
                        new LoggerConfiguration()
                            .WriteTo.Console()
                            .CreateLogger(),
                        true);
                })
                .UseOutkeepServer((context, outkeep) =>
                {
                    outkeep.Configure<CacheGrainOptions>(options =>
                    {
                        options.ExpirationPolicyEvaluationPeriod = context.Configuration.GetValue<TimeSpan>("Outkeep:DistributedCache:ExpirationPolicyEvaluationPeriod");
                    });
                })
                .RunConsoleAsync();
        }
    }
}