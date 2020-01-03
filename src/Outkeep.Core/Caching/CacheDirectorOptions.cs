using Outkeep.Core.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Core.Caching
{
    public class CacheDirectorOptions
    {
        /// <summary>
        /// The desired capacity that the cache director will attempt to meet by removing existing entries in the background.
        /// Must be equal to or lower than <see cref="MaxCapacity"/>.
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

        public bool AutomaticOvercapacityCompaction { get; set; } = true;

        [Range(typeof(TimeSpan), "00:00:00.001", "1.00:00:00", ConvertValueInInvariantCulture = true)]
        public TimeSpan OvercapacityCompactionFrequency { get; set; } = TimeSpan.FromSeconds(1);
    }
}