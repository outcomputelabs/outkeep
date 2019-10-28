﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outkeep.Client.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Client
{
    public class OutkeepClientHostedService : IHostedService
    {
        private readonly ILogger<OutkeepClientHostedService> logger;

        public OutkeepClientHostedService(ILogger<OutkeepClientHostedService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation(Resources.OutkeepClientStarted);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation(Resources.OutkeepClientStopped);

            return Task.CompletedTask;
        }
    }
}