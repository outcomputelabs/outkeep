﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outkeep.Server.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Server
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
            logger.LogInformation(Resources.OutkeepServerStarted);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation(Resources.OutkeepServerStopped);

            return Task.CompletedTask;
        }
    }
}