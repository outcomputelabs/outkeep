using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outkeep.Hosting.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Hosting
{
    internal class OutkeepServerHostedService : IHostedService
    {
        private readonly ILogger _logger;

        public OutkeepServerHostedService(ILogger<OutkeepServerHostedService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.OutkeepServerStarting(_logger);
            Log.OutkeepServerStarted(_logger);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.OutkeepServerStopping(_logger);
            Log.OutkeepServerStopped(_logger);
            return Task.CompletedTask;
        }

        private static class Log
        {
            #region OutkeepServerStarting

            private static readonly Action<ILogger, Exception?> OutkeepServerStartingAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepServerStarting)),
                    Resources.LogOutkeepServerStarting);

            public static void OutkeepServerStarting(ILogger logger) =>
                OutkeepServerStartingAction(logger, null);

            #endregion OutkeepServerStarting

            #region OutkeepServerStarted

            private static readonly Action<ILogger, Exception?> OutkeepServerStartedAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepServerStarted)),
                    Resources.LogOutkeepServerStarted);

            public static void OutkeepServerStarted(ILogger logger) =>
                OutkeepServerStartedAction(logger, null);

            #endregion OutkeepServerStarted

            #region OutkeepServerStopping

            private static readonly Action<ILogger, Exception?> OutkeepServerStoppingAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepServerStopping)),
                    Resources.LogOutkeepServerStopping);

            public static void OutkeepServerStopping(ILogger logger) =>
                OutkeepServerStoppingAction(logger, null);

            #endregion OutkeepServerStopping

            #region OutkeepServerStopped

            private static readonly Action<ILogger, Exception?> OutkeepServerStoppedAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepServerStopped)),
                    Resources.LogOutkeepServerStopped);

            public static void OutkeepServerStopped(ILogger logger) =>
                OutkeepServerStoppedAction(logger, null);

            #endregion OutkeepServerStopped
        }
    }
}