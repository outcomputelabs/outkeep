using Outkeep.HealthChecks;
using System;

namespace Orleans
{
    public static class OutkeepHealthChecksGrainFactoryExtensions
    {
        /// <summary>
        /// Gets a proxy to an <see cref="IEchoGrain"/>.
        /// </summary>
        public static IEchoGrain GetEchoGrain(this IGrainFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            return factory.GetGrain<IEchoGrain>(Guid.Empty);
        }
    }
}