using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Grains
{
    public class CacheGrainOptions
    {
        [Range(typeof(TimeSpan), "00:00:01", "00:00:29")]
        public TimeSpan ReactivePollingTimeout { get; set; } = TimeSpan.FromSeconds(20);

        [Range(typeof(TimeSpan), "00:00:01", "24:00:00")]
        public TimeSpan MaintenancePeriod { get; set; } = TimeSpan.FromSeconds(1);
    }
}