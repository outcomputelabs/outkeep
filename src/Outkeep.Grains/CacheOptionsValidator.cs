using Microsoft.Extensions.Options;
using Outkeep.Implementations.Properties;
using System;

namespace Outkeep.Grains
{
    internal class CacheOptionsValidator : IValidateOptions<CacheGrainOptions>
    {
        public ValidateOptionsResult Validate(string name, CacheGrainOptions options)
        {
            if (options.ExpirationPolicyEvaluationPeriod <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail(Resources.ExceptionXMustBeAPositiveX.Format(nameof(options.ExpirationPolicyEvaluationPeriod), nameof(TimeSpan)));

            return ValidateOptionsResult.Success;
        }
    }
}