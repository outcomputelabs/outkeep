using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Hosting
{
    public class OutkeepServerHostedService : IHostedService
    {
        private readonly ILogger<OutkeepServerHostedService> logger;

        public OutkeepServerHostedService(ILogger<OutkeepServerHostedService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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