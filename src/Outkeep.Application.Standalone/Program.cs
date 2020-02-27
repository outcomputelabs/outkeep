using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Statistics;
using Outkeep.Caching;
using Outkeep.Core;
using Serilog;
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
                    outkeep.Configure<CacheOptions>(options =>
                    {
                        context.Configuration.GetSection("Outkeep:DistributedCaching").Bind(options);
                    });

                    outkeep.UseStandaloneClustering();
                    outkeep.UseOutkeepDashboard(options =>
                    {
                        options.Brand = $"{nameof(Outkeep)} {nameof(Standalone)}";
                    });

                    outkeep.UseHttpApi(options =>
                    {
                        context.Configuration.Bind("Outkeep:Http", options);
                    });

                    outkeep.ConfigureSilo(silo =>
                    {
                        silo.UsePerfCounterEnvironmentStatistics();
                    });

                    outkeep.ConfigureServices(services =>
                    {
                        services.AddNullGrainStorage(OutkeepProviderNames.OutkeepCache);
                    });
                })
                .UseConsoleTitle()
                .RunConsoleAsync();
        }
    }
}