using Microsoft.Extensions.Logging;
using Outkeep.Grains.Properties;
using System;

namespace Outkeep.Grains
{
    /// <summary>
    /// High-performance logging extensions.
    /// </summary>
    public static class LoggerExtensions
    {
        #region Echo

        public static void Echo(this ILogger logger, string message) =>
            _echo(logger, message, null);

        private static readonly Action<ILogger, string, Exception?> _echo =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(0, nameof(Echo)),
                Resources.Log_Echo_X);

        #endregion Echo
    }
}