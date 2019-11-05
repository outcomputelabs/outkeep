using Outkeep.Hosting.Properties;
using System;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// High-performance logging extension methods for high-frequency log calls.
    /// </summary>
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> OutkeepServerStarting = LoggerMessage.Define(LogLevel.Information, new EventId(0, nameof(OutkeepServerStarting)), Resources.LogOutkeepServerStarting);
        public static void LogOutkeepServerStarting(this ILogger logger) => OutkeepServerStarting(logger, null);

        private static readonly Action<ILogger, Exception> OutkeepServerStarted = LoggerMessage.Define(LogLevel.Information, new EventId(0, nameof(OutkeepServerStarted)), Resources.LogOutkeepServerStarted);
        public static void LogOutkeepServerStarted(this ILogger logger) => OutkeepServerStarted(logger, null);

        private static readonly Action<ILogger, Exception> OutkeepServerStopping = LoggerMessage.Define(LogLevel.Information, new EventId(0, nameof(OutkeepServerStopping)), Resources.LogOutkeepServerStopping);
        public static void LogOutkeepServerStopping(this ILogger logger) => OutkeepServerStopping(logger, null);

        private static readonly Action<ILogger, Exception> OutkeepServerStopped = LoggerMessage.Define(LogLevel.Information, new EventId(0, nameof(OutkeepServerStopped)), Resources.LogOutkeepServerStopped);
        public static void LogOutkeepServerStopped(this ILogger logger) => OutkeepServerStopped(logger, null);
    }
}