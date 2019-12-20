using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Core.Caching
{
    public class CacheDirectorOptions
    {
        [Range(1, int.MaxValue)]
        public long Capacity { get; set; }

        public TimeSpan ExpirationScanFrequency { get; set; }

        [Range(0, 1)]
        public double TargetCompactionRatio { get; set; } = 0.8;
    }
}