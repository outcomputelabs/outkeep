using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Outkeep.Application.Standalone
{
    internal static class ConsoleTitleServiceHostBuilderExtensions
    {
        public static IHostBuilder UseConsoleTitle(this IHostBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddHostedService<ConsoleTitleService>();
            });
        }
    }
}