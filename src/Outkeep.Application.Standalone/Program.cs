using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outkeep.Grains;
using Outkeep.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Outkeep.Application.Standalone
{
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
                        options.ExpirationPolicyEvaluationPeriod = context.Configuration.GetValue<TimeSpan>("Outkeep:DistributedCache:ExpirationPolicyEvaluationPeriod");
                    });

                    outkeep.UseStandaloneClustering();
                    outkeep.UseHttpApi(options =>
                    {
                        options.ApiUri = new Uri(context.Configuration["Outkeep:Http:ApiUri"]);
                        options.SwaggerUiUri = new Uri(context.Configuration["Outkeep:Http:SwaggerUiUri"], UriKind.Relative);
                    });
                })
                .RunConsoleAsync();
        }
    }
}