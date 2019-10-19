using System;

namespace Outkeep.Implementations
{
    public class DistributedCacheOptions
    {
        public TimeSpan PolicyExecutionPeriod { get; set; } = TimeSpan.FromSeconds(1);
    }
}