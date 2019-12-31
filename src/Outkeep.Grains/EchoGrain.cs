using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Outkeep.Interfaces;
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
            _logger.Echo(message);

            return new ValueTask<string>(message);
        }
    }
}