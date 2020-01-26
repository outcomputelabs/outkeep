using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Outkeep.Grains.Echo;
using Outkeep.Grains.Properties;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    /// <summary>
    /// Grain that eachoes messages back to the caller.
    /// For use with health checking and performance testing.
    /// </summary>
    [StatelessWorker(1)]
    internal class EchoGrain : Grain, IEchoGrain
    {
        private readonly ILogger _logger;

        public EchoGrain(ILogger<EchoGrain> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public ValueTask<string> EchoAsync(string message)
        {
            Log.Echo(_logger, message);

            return new ValueTask<string>(message);
        }

        private static class Log
        {
            #region Echo

            public static void Echo(ILogger logger, string message) =>
                _echo(logger, message, null);

            private static readonly Action<ILogger, string, Exception?> _echo =
                LoggerMessage.Define<string>(
                    LogLevel.Debug,
                    new EventId(0, nameof(Echo)),
                    Resources.Log_Echo_X);

            #endregion Echo
        }
    }
}