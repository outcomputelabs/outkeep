namespace Outkeep.Core.Governance
{
    public class MemoryPressureOptions
    {
        /// <summary>
        /// Memory is under pressure if the ratio between available memory and total memory falls below this threshold.
        /// Defaults to 0.1 (10%).
        /// </summary>
        public double? LowMemoryThreshold { get; set; } = 0.1;

        /// <summary>
        /// Memory is under pressure if available bytes fall below this threshold.
        /// </summary>
        public long? LowMemoryBytesThreshold { get; set; }
    }
}