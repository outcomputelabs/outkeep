using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Hosting
{
    internal class OutkeepServerHostedService : IHostedService
    {
        private readonly ILogger<OutkeepServerHostedService> logger;
        private readonly IHost host;

        public OutkeepServerHostedService(ILogger<OutkeepServerHostedService> logger, IOutkeepServerBuilder outkeep)
        {
            this.logger = logger;

            var builder = new HostBuilder()
                .ConfigureContainer((context, container) =>
                {
                    
                });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogOutkeepServerStarting();

            await host.StartAsync().ConfigureAwait(false);

            logger.LogOutkeepServerStarted();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogOutkeepServerStopping();

            await host.StopAsync().ConfigureAwait(false);

            logger.LogOutkeepServerStopped();
        }
    }
}