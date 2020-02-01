using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Governance.Memory
{
    public class MemoryGovernanceOptions
    {
        /// <summary>
        /// Memory is under pressure if the ratio between available memory and total memory falls below this threshold.
        /// Defaults to 0.1 (10%).
        /// </summary>
        [Range(0.0, 1.0)]
        public double LowMemoryThreshold { get; set; } = 0.1;

        /// <summary>
        /// Memory is under pressure if available bytes fall below this threshold.
        /// </summary>
        [Range(0, long.MaxValue)]
        public long LowMemoryBytesThreshold { get; set; } = 0L;

        /// <summary>
        /// Fraction of grains that will be collected upon a memory pressure signal.
        /// Defaults to 0.1 (10%).
        /// </summary>
        [Range(0.0, 1.0)]
        public double GrainDeactivationRatio { get; set; } = 0.1;

        /// <summary>
        /// Interval at which weak activations will be collected.
        /// </summary>
        [Range(typeof(TimeSpan), "00:00:00.001", "24.00:00:00")]
        public TimeSpan WeakActivationCollectionInterval { get; set; } = TimeSpan.FromSeconds(1);
    }
}