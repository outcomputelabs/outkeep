using System;

namespace Outkeep.Implementations
{
    public class CacheGrainOptions
    {
        public TimeSpan ExpirationPolicyEvaluationPeriod { get; set; } = TimeSpan.FromSeconds(1);
    }
}