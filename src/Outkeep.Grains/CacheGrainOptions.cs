using System;

namespace Outkeep.Grains
{
    public class CacheGrainOptions
    {
        public TimeSpan ReactivePollingTimeout { get; set; } = TimeSpan.FromSeconds(20);
    }
}