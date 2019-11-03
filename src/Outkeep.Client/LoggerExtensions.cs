using System;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> OutkeepClientStarted = LoggerMessage.Define(
            LogLevel.Information, new EventId(1, nameof(OutkeepClientStarted)), "Outkeep Client Started");

        public static void LogOutkeepClientStarted(this ILogger logger) => OutkeepClientStarted(logger, null);

        private static readonly Action<ILogger, Exception> OutkeepClientStopped = LoggerMessage.Define(
            LogLevel.Information, new EventId(2, nameof(OutkeepClientStopped)), "Outkeep Client Stopped");

        public static void LogOutkeepClientStopped(this ILogger logger) => OutkeepClientStopped(logger, null);
    }
}