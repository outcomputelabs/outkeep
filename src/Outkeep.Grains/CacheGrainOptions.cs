using System;

namespace Outkeep.Grains
{
    public class CacheGrainOptions
    {
        public TimeSpan ReactivePollingTimeout { get; set; } = TimeSpan.FromSeconds(20);

        public TimeSpan MaintenancePeriod { get; set; } = TimeSpan.FromSeconds(1);
    }
}