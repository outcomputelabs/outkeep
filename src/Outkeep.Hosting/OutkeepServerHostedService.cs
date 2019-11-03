using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Hosting
{
    internal class OutkeepServerHostedService : IHostedService
    {
        private readonly ILogger<OutkeepServerHostedService> logger;

        public OutkeepServerHostedService(ILogger<OutkeepServerHostedService> logger)
        {
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogOutkeepServerStarted();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogOutkeepServerStopped();

            return Task.CompletedTask;
        }
    }
}