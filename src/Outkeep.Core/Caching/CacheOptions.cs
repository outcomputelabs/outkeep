using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Options for Outkeep caching.
    /// </summary>
    public sealed class CacheOptions
    {
        /// <summary>
        /// The maximum capacity that the director will respect by refusing new entries.
        /// </summary>
        [Range(1, long.MaxValue)]
        public long Capacity { get; set; }

        /// <summary>
        /// The frequency at which to perform an expiration scan.
        /// </summary>
        [Range(typeof(TimeSpan), "00:00:00.001", "1.00:00:00", ConvertValueInInvariantCulture = true)]
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromSeconds(1);
    }
}