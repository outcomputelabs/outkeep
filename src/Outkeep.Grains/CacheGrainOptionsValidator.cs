using Microsoft.Extensions.Options;
using Outkeep.Core;
using Outkeep.Grains.Properties;
using System;

namespace Outkeep.Grains
{
    public class CacheGrainOptionsValidator : IValidateOptions<CacheGrainOptions>
    {
        public ValidateOptionsResult Validate(string name, CacheGrainOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ExpirationPolicyEvaluationPeriod <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail(Resources.Exception_XMustBeAPositiveX.Format(nameof(options.ExpirationPolicyEvaluationPeriod), nameof(TimeSpan)));

            if (options.ReactivePollGracefulTimeout <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail(Resources.Exception_XMustBeAPositiveX.Format(nameof(options.ReactivePollGracefulTimeout), nameof(TimeSpan)));

            return ValidateOptionsResult.Success;
        }
    }
}