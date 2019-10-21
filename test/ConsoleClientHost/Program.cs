using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ConsoleClientHost
{
    internal class Program
    {
        private static Task Main()
        {
            return new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .UseOutkeepClient(builder =>
                {
                })
                .RunConsoleAsync();
        }
    }
}