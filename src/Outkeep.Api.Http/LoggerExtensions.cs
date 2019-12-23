using Outkeep.Api.Http.Properties;
using System;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception?> OutkeepHttpApiStarting =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepHttpApiStarting)),
                Resources.OutkeepHttpApiStarting);

        public static void LogOutkeepHttpApiStarting(this ILogger logger) =>
            OutkeepHttpApiStarting(logger, null);

        private static readonly Action<ILogger, Exception?> OutkeepHttpApiStarted =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepHttpApiStarted)),
                Resources.OutkeepHttpApiStarted);

        public static void LogOutkeepHttpApiStarted(this ILogger logger) =>
            OutkeepHttpApiStarted(logger, null);

        private static readonly Action<ILogger, Exception?> OutkeepHttpApiStopping =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepHttpApiStopping)),
                Resources.OutkeepHttpApiStopping);

        public static void LogOutkeepHttpApiStopping(this ILogger logger) =>
            OutkeepHttpApiStopping(logger, null);

        private static readonly Action<ILogger, Exception?> OutkeepHttpApiStopped =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepHttpApiStopped)),
                Resources.OutkeepHttpApiStopped);

        public static void LogOutkeepHttpApiStopped(this ILogger logger) =>
            OutkeepHttpApiStopped(logger, null);

        private static readonly Action<ILogger, Guid, Exception?> OutkeepActivityStarting =
            LoggerMessage.Define<Guid>(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepActivityStarting)),
                Resources.StartingActivity_X);

        public static void LogOutkeepActivityStarting(this ILogger logger, Guid activityId) =>
            OutkeepActivityStarting(logger, activityId, null);
    }
}