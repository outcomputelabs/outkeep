using System;

namespace Outkeep.Implementations
{
    public class DistributedCacheOptions
    {
        public TimeSpan ExpirationPolicyEvaluationPeriod { get; set; } = TimeSpan.FromSeconds(1);
    }
}