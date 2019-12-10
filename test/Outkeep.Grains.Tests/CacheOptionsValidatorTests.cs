using Outkeep.Grains.Properties;
using System;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class CacheOptionsValidatorTests
    {
        [Fact]
        public void RefusesNegativeExpirationPolicyEvaluationPeriod()
        {
            var options = new CacheGrainOptions
            {
                ExpirationPolicyEvaluationPeriod = TimeSpan.FromTicks(-1)
            };
            var validator = new CacheOptionsValidator();

            var result = validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Equal(Resources.Exception_XMustBeAPositiveX.Format(nameof(options.ExpirationPolicyEvaluationPeriod), nameof(TimeSpan)), result.FailureMessage);
        }

        [Fact]
        public void RefusesZeroExpirationPolicyEvaluationPeriod()
        {
            var options = new CacheGrainOptions
            {
                ExpirationPolicyEvaluationPeriod = TimeSpan.Zero
            };
            var validator = new CacheOptionsValidator();

            var result = validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Equal(Resources.Exception_XMustBeAPositiveX.Format(nameof(options.ExpirationPolicyEvaluationPeriod), nameof(TimeSpan)), result.FailureMessage);
        }

        [Fact]
        public void AcceptsPositiveExpirationPolicyEvaluationPeriod()
        {
            var options = new CacheGrainOptions
            {
                ExpirationPolicyEvaluationPeriod = TimeSpan.FromTicks(1)
            };
            var validator = new CacheOptionsValidator();

            var result = validator.Validate(null, options);

            Assert.True(result.Succeeded);
        }
    }
}