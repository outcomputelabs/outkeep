using Outkeep.Core.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Options for <see cref="CacheDirector"/>.
    /// </summary>
    public sealed class CacheDirectorOptions
    {
        /// <summary>
        /// The desired capacity that the cache director will attempt to meet by removing existing entries in the background.
        /// Must be equal to or lower than <see cref="MaxCapacity"/>.
        /// Ignored when <see cref="AutomaticOvercapacityCompaction"/> is <see cref="false"/>.
        /// </summary>
        [Range(1, int.MaxValue)]
        public long TargetCapacity { get; set; }

        /// <summary>
        /// The maximum capacity that the director will respect by refusing new entries.
        /// Must be equal to or greater than <see cref="TargetCapacity"/>.
        /// </summary>
        [Range(1, int.MaxValue)]
        [GreaterThanOrEqual(nameof(TargetCapacity))]
        public long MaxCapacity { get; set; }

        /// <summary>
        /// The frequency at which to perform an expiration scan.
        /// </summary>
        [Range(typeof(TimeSpan), "00:00:00.001", "1.00:00:00", ConvertValueInInvariantCulture = true)]
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Whether to perform automatic background eviction of items to keep used capacity under <see cref="TargetCapacity"/>.
        /// </summary>
        public bool AutomaticOvercapacityCompaction { get; set; } = true;

        /// <summary>
        /// The frequency at which to perform automatic background eviction to keep used capacity under <see cref="TargetCapacity"/>.
        /// </summary>
        [Range(typeof(TimeSpan), "00:00:00.001", "1.00:00:00", ConvertValueInInvariantCulture = true)]
        public TimeSpan OvercapacityCompactionFrequency { get; set; } = TimeSpan.FromSeconds(1);
    }
}