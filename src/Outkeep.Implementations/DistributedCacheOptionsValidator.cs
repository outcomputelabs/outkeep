using Microsoft.Extensions.Options;
using Outkeep.Implementations.Properties;
using System;

namespace Outkeep.Implementations
{
    internal class DistributedCacheOptionsValidator : IValidateOptions<DistributedCacheOptions>
    {
        public ValidateOptionsResult Validate(string name, DistributedCacheOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ExpirationPolicyEvaluationPeriod <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail(Resources.ExceptionXMustBeAPositiveX.Format(nameof(options.ExpirationPolicyEvaluationPeriod), nameof(TimeSpan)));

            return ValidateOptionsResult.Success;
        }
    }
}