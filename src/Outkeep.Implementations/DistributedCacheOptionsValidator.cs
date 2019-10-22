using Microsoft.Extensions.Options;
using Outkeep.Implementations.Properties;
using Outkeep.Interfaces;
using System;

namespace Outkeep.Implementations
{
    public class DistributedCacheOptionsValidator : IValidateOptions<DistributedCacheOptions>
    {
        public ValidateOptionsResult Validate(string name, DistributedCacheOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ExpirationPolicyEvaluationPeriod <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail(Resources.X_MustBeAPositive_X.Format(nameof(options.ExpirationPolicyEvaluationPeriod), nameof(TimeSpan)));

            return ValidateOptionsResult.Success;
        }
    }
}