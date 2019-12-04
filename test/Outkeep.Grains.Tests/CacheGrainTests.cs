using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Core;
using Orleans.Timers;
using Outkeep.Core;
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
            var options = Options.Create(new CacheGrainOptions { });
            var logger = Mock.Of<ILogger<CacheGrain>>();
            var timers = Mock.Of<ITimerRegistry>();
            var storage = Mock.Of<ICacheStorage>();
            var clock = Mock.Of<ISystemClock>();
            var identity = Mock.Of<IGrainIdentity>();
            var grain = new CacheGrain(options, logger, timers, storage, clock, identity);

            await grain.OnActivateAsync().ConfigureAwait(false);

            Mock.Get(timers).Verify(x => x.RegisterTimer(grain, It.IsAny<Func<object, Task>>(), null, options.Value.ExpirationPolicyEvaluationPeriod, options.Value.ExpirationPolicyEvaluationPeriod));
        }

        [Fact]
        public async Task GetReturnsNullValueFromStorage()
        {
            // arrange
            var key = "SomeKey";
            var options = Options.Create(new CacheGrainOptions { });
            var logger = Mock.Of<ILogger<CacheGrain>>();
            var timers = Mock.Of<ITimerRegistry>();

            var storage = Mock.Of<ICacheStorage>(x =>
                x.ReadAsync(key, default) == Task.FromResult<CacheItem?>(null));

            var clock = Mock.Of<ISystemClock>();

            var identity = Mock.Of<IGrainIdentity>(x =>
                x.PrimaryKeyString == key);

            var grain = new CacheGrain(options, logger, timers, storage, clock, identity);
            await grain.OnActivateAsync().ConfigureAwait(false);

            // act
            var result = await grain.GetAsync().ConfigureAwait(false);

            // assert
            Assert.Null(result.Value);
            Mock.Get(storage).VerifyAll();
        }

        [Fact]
        public async Task GetReturnsCorrectValueFromStorage()
        {
            // arrange
            var key = "SomeKey";
            var options = Options.Create(new CacheGrainOptions { });
            var logger = Mock.Of<ILogger<CacheGrain>>();
            var timers = Mock.Of<ITimerRegistry>();

            var value = Guid.NewGuid().ToByteArray();
            var storage = Mock.Of<ICacheStorage>(x =>
                x.ReadAsync(key, default) == Task.FromResult<CacheItem?>(new CacheItem(value, null, null)));

            var clock = Mock.Of<ISystemClock>();

            var identity = Mock.Of<IGrainIdentity>(x =>
                x.PrimaryKeyString == key);

            var grain = new CacheGrain(options, logger, timers, storage, clock, identity);
            await grain.OnActivateAsync().ConfigureAwait(false);

            // act
            var result = await grain.GetAsync().ConfigureAwait(false);

            // assert
            Assert.NotNull(result.Value);
            Assert.Same(value, result.Value);
            Mock.Get(storage).VerifyAll();
        }
    }
}