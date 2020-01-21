using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outkeep.Core;
using Outkeep.Grains;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Outkeep.Application.Standalone
{
    [ExcludeFromCodeCoverage]
    internal sealed class Program
    {
        private Program()
        {
        }

        private static Task Main()
        {
            return new HostBuilder()
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
                .ConfigureServices(services =>
                {
                    services.Configure<ConsoleLifetimeOptions>(options =>
                    {
                        options.SuppressStatusMessages = true;
                    });
                })
                .UseOutkeepServer((context, outkeep) =>
                {
                    outkeep.Configure<CacheGrainOptions>(options =>
                    {
                        options.ReactivePollingTimeout = context.Configuration.GetValue<TimeSpan>("Outkeep:DistributedCaching:ReactivePollingTimeout");
                    });

                    outkeep.UseStandaloneClustering();
                    outkeep.UseHttpApi(options =>
                    {
                        options.ApiUri = new Uri(context.Configuration["Outkeep:Http:ApiUri"]);
                    });
                })
                .UseConsoleTitle()
                .RunConsoleAsync();
        }
    }
}