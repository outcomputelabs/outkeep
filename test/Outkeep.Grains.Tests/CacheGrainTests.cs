using Microsoft.Extensions.Options;
using Moq;
using Orleans.Timers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class CacheGrainTests
    {
        [Fact]
        public async Task RegistersTimerOnActivation()
        {
            var options = Options.Create(new CacheGrainOptions
            {
            });
            var timers = Mock.Of<ITimerRegistry>();
            var grain = new CacheGrain(options, timers);

            await grain.OnActivateAsync().ConfigureAwait(false);

            Mock.Get(timers).Verify(x => x.RegisterTimer(grain, It.IsAny<Func<object, Task>>(), null, options.Value.ExpirationPolicyEvaluationPeriod, options.Value.ExpirationPolicyEvaluationPeriod));
        }
    }
}