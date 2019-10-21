using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ConsoleServerHost
{
    public static class Program
    {
        public static Task Main()
        {
            return new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .UseOutkeepServer(builder =>
                {
                })
                .RunConsoleAsync();
        }
    }
}