using System;

namespace Outkeep.Grains
{
    public class CacheGrainOptions
    {
        public TimeSpan ExpirationPolicyEvaluationPeriod { get; set; } = TimeSpan.FromSeconds(1);
    }
}