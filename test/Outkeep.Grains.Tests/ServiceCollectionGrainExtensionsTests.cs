using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class ServiceCollectionGrainExtensionsTests
    {
        [Fact]
        public void AddCacheGrainOptionsAddsServices()
        {
            // arrange
            var services = new ServiceCollection();
            var value = new TimeSpan(1, 2, 3, 4, 5);

            // act
            services.AddCacheGrainOptions(x =>
            {
                x.ExpirationPolicyEvaluationPeriod = value;
            });

            // assert
            var provider = services.BuildServiceProvider();

            var options = provider.GetService<IOptions<CacheGrainOptions>>()?.Value;
            Assert.NotNull(options);
            Assert.Equal(value, options.ExpirationPolicyEvaluationPeriod);

            var validator = provider.GetService<IValidateOptions<CacheGrainOptions>>();
            Assert.NotNull(validator);
            Assert.IsType<CacheOptionsValidator>(validator);
        }
    }
}