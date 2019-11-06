using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Api.Rest
{
    internal class RestApiStartupTask : IStartupTask
    {
        private readonly ILogger<RestApiStartupTask> logger;

        public RestApiStartupTask(ILogger<RestApiStartupTask> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            logger.LogOutkeepRestApiStarted();

            return Task.CompletedTask;
        }
    }
}