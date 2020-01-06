using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outkeep.Client.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Client
{
    internal class OutkeepClientHostedService : IHostedService
    {
        private readonly ILogger<OutkeepClientHostedService> _logger;

        public OutkeepClientHostedService(ILogger<OutkeepClientHostedService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.OutkeepClientStarted(_logger);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.OutkeepClientStopped(_logger);

            return Task.CompletedTask;
        }

        private static class Log
        {
            #region OutkeepClientStarted

            private static readonly Action<ILogger, Exception?> OutkeepClientStartedAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepClientStarted)),
                    Resources.Log_OutkeepClientStarted);

            public static void OutkeepClientStarted(ILogger logger) =>
                OutkeepClientStartedAction(logger, null);

            #endregion OutkeepClientStarted

            #region OutkeepClientStopped

            private static readonly Action<ILogger, Exception?> OutkeepClientStoppedAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepClientStopped)),
                    Resources.Log_OutkeepClientStopped);

            public static void OutkeepClientStopped(ILogger logger) =>
                OutkeepClientStoppedAction(logger, null);

            #endregion OutkeepClientStopped
        }
    }
}