using Microsoft.Extensions.Options;
using System;

namespace Outkeep.Implementations
{
    internal class DistributedCacheOptionsValidator : IValidateOptions<DistributedCacheOptions>
    {
        public ValidateOptionsResult Validate(string name, DistributedCacheOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ExpirationPolicyEvaluationPeriod <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail($"{nameof(options.ExpirationPolicyEvaluationPeriod)} must be a positive {nameof(TimeSpan)}");

            return ValidateOptionsResult.Success;
        }
    }
}