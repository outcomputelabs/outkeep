using Outkeep.Api.Rest.Properties;
using System;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> OutkeepRestApiStarted = LoggerMessage.Define(LogLevel.Information, new EventId(0, nameof(OutkeepRestApiStarted)), Resources.OutkeepRestApiStarted);

        public static void LogOutkeepRestApiStarted(this ILogger logger) => OutkeepRestApiStarted(logger, null);
    }
}