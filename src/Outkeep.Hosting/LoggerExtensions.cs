using Outkeep.Hosting.Properties;
using System;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> OutkeepServerStarted = LoggerMessage.Define(
            LogLevel.Information, new EventId(0, nameof(OutkeepServerStarted)), Resources.LogOutkeepServerStarted);

        public static void LogOutkeepServerStarted(this ILogger logger) => OutkeepServerStarted(logger, null);

        private static readonly Action<ILogger, Exception> OutkeepServerStopped = LoggerMessage.Define(
            LogLevel.Information, new EventId(0, nameof(OutkeepServerStopped)), Resources.LogOutkeepServerStopped);

        public static void LogOutkeepServerStopped(this ILogger logger) => OutkeepServerStopped(logger, null);
    }
}