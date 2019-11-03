using System;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> OutkeepServerStarted = LoggerMessage.Define(
            LogLevel.Information, new EventId(1, nameof(OutkeepServerStarted)), "Outkeep Server Started");

        public static void LogOutkeepServerStarted(this ILogger logger) => OutkeepServerStarted(logger, null);

        private static readonly Action<ILogger, Exception> OutkeepServerStopped = LoggerMessage.Define(
            LogLevel.Information, new EventId(2, nameof(OutkeepServerStopped)), "Outkeep Server Stopped");

        public static void LogOutkeepServerStopped(this ILogger logger) => OutkeepServerStopped(logger, null);
    }
}