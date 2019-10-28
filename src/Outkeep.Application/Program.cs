using Microsoft.Extensions.Hosting;
using System;
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
                })
                .ConfigureLogging((context, logging) =>
                {
                })
                .ConfigureServices((context, services) =>
                {
                })
                .RunConsoleAsync();
        }
    }
}
