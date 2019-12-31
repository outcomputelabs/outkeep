using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Concurrency;
using Orleans.Core;
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
            var timers = new NullTimerRegistry();
            var storage = Mock.Of<ICacheStorage>();
            var clock = Mock.Of<ISystemClock>();
            var identity = Mock.Of<IGrainIdentity>();
            var grain = new CacheGrain(options, logger, timers, storage, clock, identity);

            await grain.OnActivateAsync().ConfigureAwait(false);

            Assert.Single(timers.GetEntries(grain), x =>
                x.AsyncCallback != null &&
                x.State == null &&
                x.DueTime == options.Value.ExpirationPolicyEvaluationPeriod &&
                x.Period == options.Value.ExpirationPolicyEvaluationPeriod);
        }

        [Fact]
        public async Task GetReturnsNullValueFromStorage()
        {
            // arrange
            var key = "SomeKey";
            var options = Options.Create(new CacheGrainOptions { });
            var logger = Mock.Of<ILogger<CacheGrain>>();
            var timers = new NullTimerRegistry();

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
            var timers = new NullTimerRegistry();

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

        [Fact]
        public async Task GetReturnsCorrectValueFromMemory()
        {
            // arrange
            var key = "SomeKey";
            var value = Guid.NewGuid().ToByteArray();

            var storage = Mock.Of<ICacheStorage>();
            Mock.Get(storage).Setup(x => x.ReadAsync(key, default)).Returns(Task.FromResult<CacheItem?>(new CacheItem(value, null, null)));

            var grain = new CacheGrain(
                Options.Create(new CacheGrainOptions { }),
                new NullLogger<CacheGrain>(),
                new NullTimerRegistry(),
                storage,
                Mock.Of<ISystemClock>(),
                Mock.Of<IGrainIdentity>(x => x.PrimaryKeyString == key));

            // act - this will load the value from storage
            await grain.OnActivateAsync().ConfigureAwait(false);
            var result = await grain.GetAsync().ConfigureAwait(false);

            // assert
            Assert.Same(value, result.Value);

            // arrange - clear storage
            Mock.Get(storage).Setup(x => x.ReadAsync(key, default)).Returns(Task.FromResult<CacheItem?>(null));

            // act - this must reuse the value in memory
            result = await grain.GetAsync().ConfigureAwait(false);

            // assert
            Assert.Same(value, result.Value);
        }

        [Fact]
        public async Task RemoveClearsValueFromMemoryAndStorage()
        {
            // arrange
            var key = "SomeKey";
            var options = Options.Create(new CacheGrainOptions { });
            var logger = new NullLogger<CacheGrain>();
            var timers = new NullTimerRegistry();
            var storage = new MemoryCacheStorage();
            var clock = Mock.Of<ISystemClock>();
            var identity = Mock.Of<IGrainIdentity>(x => x.PrimaryKeyString == key);
            var grain = new CacheGrain(options, logger, timers, storage, clock, identity);

            // act - set the value
            var value = Guid.NewGuid().ToByteArray();
            var absolute = DateTimeOffset.UtcNow;
            var sliding = TimeSpan.MaxValue;
            await grain.OnActivateAsync().ConfigureAwait(false);
            await grain.SetAsync(new Immutable<byte[]?>(value), absolute, sliding).ConfigureAwait(false);

            // assert value is set in storage
            var stored = await storage.ReadAsync(key).ConfigureAwait(false);
            Assert.Same(value, stored.Value.Value);
            Assert.Equal(absolute, stored.Value.AbsoluteExpiration);
            Assert.Equal(sliding, stored.Value.SlidingExpiration);

            // assert value is set in memory
            var result = await grain.GetAsync().ConfigureAwait(false);
            Assert.Same(value, result.Value);

            // act - clear the value
            await grain.RemoveAsync().ConfigureAwait(false);

            // assert value is cleared from storage
            Assert.False((await storage.ReadAsync(key).ConfigureAwait(false)).HasValue);

            // arrange - add dummy value to storage
            await storage.WriteAsync(key, new CacheItem(value, absolute, sliding)).ConfigureAwait(false);

            // assert value is cleared from memory and not reloaded
            result = await grain.GetAsync().ConfigureAwait(false);
            Assert.Null(result.Value);
        }
    }
}