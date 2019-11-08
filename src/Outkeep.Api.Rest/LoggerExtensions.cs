using Outkeep.Api.Rest.Properties;
using System;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> OutkeepRestApiStarting =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepRestApiStarting)),
                Resources.OutkeepRestApiStarting);

        public static void LogOutkeepRestApiStarting(this ILogger logger) =>
            OutkeepRestApiStarting(logger, null);

        private static readonly Action<ILogger, Exception> OutkeepRestApiStarted =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepRestApiStarted)),
                Resources.OutkeepRestApiStarted);

        public static void LogOutkeepRestApiStarted(this ILogger logger) =>
            OutkeepRestApiStarted(logger, null);

        private static readonly Action<ILogger, Exception> OutkeepRestApiStopping =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepRestApiStopping)),
                Resources.OutkeepRestApiStopping);

        public static void LogOutkeepRestApiStopping(this ILogger logger) =>
            OutkeepRestApiStopping(logger, null);

        private static readonly Action<ILogger, Exception> OutkeepRestApiStopped =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepRestApiStopped)),
                Resources.OutkeepRestApiStopped);

        public static void LogOutkeepRestApiStopped(this ILogger logger) =>
            OutkeepRestApiStopped(logger, null);

        private static readonly Action<ILogger, Guid, Exception> OutkeepActivityStarting =
            LoggerMessage.Define<Guid>(
                LogLevel.Information,
                new EventId(0, nameof(OutkeepActivityStarting)),
                Resources.StartingActivity_X);

        public static void LogOutkeepActivityStarting(this ILogger logger, Guid activityId) =>
            OutkeepActivityStarting(logger, activityId, null);
    }
}