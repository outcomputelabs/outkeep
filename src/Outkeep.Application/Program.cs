using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading.Tasks;

namespace Outkeep.Application
{
    internal sealed class Program
    {
        private static Task Main(string[] args)
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
                        .AddUserSecrets<Program>()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddSerilog(new LoggerConfiguration()
                        .WriteTo.Console()
                        .CreateLogger());
                })
                .ConfigureServices((context, services) =>
                {
                })
                .UseOutkeepServer((context, outkeep) =>
                {
                    outkeep.ConfigureSilo(orleans =>
                    {
                    });
                })
                .RunConsoleAsync();
        }
    }
}