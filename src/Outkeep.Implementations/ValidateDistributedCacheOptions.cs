using Microsoft.Extensions.Options;
using Outkeep.Implementations.Properties;
using Outkeep.Interfaces;
using System;

namespace Outkeep.Implementations
{
    public class ValidateDistributedCacheOptions : IValidateOptions<DistributedCacheOptions>
    {
        public ValidateOptionsResult Validate(string name, DistributedCacheOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.PolicyExecutionPeriod <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail(Resources.X_MustBeAPositive_X.Format(nameof(options.PolicyExecutionPeriod), nameof(TimeSpan)));

            return ValidateOptionsResult.Success;
        }
    }
}