using Outkeep.HealthChecks;
using System;

namespace Orleans
{
    /// <summary>
    /// Quality-of-life extension methods for <see cref="IGrainFactory"/>.
    /// </summary>
    public static class OutkeepGrainFactoryExtensions
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