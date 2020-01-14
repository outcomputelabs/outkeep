using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Timers;
using Outkeep.Core;
using Outkeep.Core.Caching;
using Outkeep.Core.Storage;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class CacheGrainTests
    {
        [Fact]
        public async Task GetReturnsNullValueFromStorage()
        {
            // arrange
            var key = "SomeKey";
            var options = Options.Create(new CacheGrainOptions { });
            var logger = Mock.Of<ILogger<CacheGrain>>();
            var storage = Mock.Of<ICacheStorage>(x => x.ReadAsync(key, default) == Task.FromResult<CacheItem?>(null));
            var clock = Mock.Of<ISystemClock>();
            var identity = Mock.Of<IGrainIdentity>(x => x.PrimaryKeyString == key);
            var director = Mock.Of<ICacheDirector>();
            var timers = Mock.Of<ITimerRegistry>();
            var context = Mock.Of<ICacheGrainContext>();
            var grain = new CacheGrain(context, options, logger, storage, clock, identity, director, timers);
            await grain.OnActivateAsync().ConfigureAwait(false);

            // act
            var result = await grain.GetAsync().ConfigureAwait(false);

            // assert
            Assert.Null(result.Value.Value);
            Mock.Get(storage).VerifyAll();
        }

        [Fact]
        public async Task GetReturnsCorrectValueFromStorage()
        {
            // arrange
            var key = "SomeKey";
            var options = Options.Create(new CacheGrainOptions { });
            var logger = Mock.Of<ILogger<CacheGrain>>();
            var value = Guid.NewGuid().ToByteArray();
            var storage = Mock.Of<ICacheStorage>(x => x.ReadAsync(key, default) == Task.FromResult<CacheItem?>(new CacheItem(value, null, null)));
            var clock = Mock.Of<ISystemClock>();
            var identity = Mock.Of<IGrainIdentity>(x => x.PrimaryKeyString == key);

            var entry = Mock.Of<ICacheEntry>();
            Mock.Get(entry).Setup(x => x.Commit()).Returns(entry);

            var director = Mock.Of<ICacheDirector>(x => x.CreateEntry(key, value.Length + IntPtr.Size) == entry);
            var timers = Mock.Of<ITimerRegistry>();
            var context = Mock.Of<ICacheGrainContext>();

            var grain = new CacheGrain(context, options, logger, storage, clock, identity, director, timers);
            await grain.OnActivateAsync().ConfigureAwait(false);

            // act
            var result = await grain.GetAsync().ConfigureAwait(false);

            // assert
            Assert.NotNull(result.Value.Value);
            Assert.Same(value, result.Value.Value);
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

            var entry = Mock.Of<ICacheEntry>();
            Mock.Get(entry).Setup(x => x.Commit()).Returns(entry);

            var director = Mock.Of<ICacheDirector>(x => x.CreateEntry(key, value.Length + IntPtr.Size) == entry);
            var timers = Mock.Of<ITimerRegistry>();
            var context = Mock.Of<ICacheGrainContext>();

            var grain = new CacheGrain(
                context,
                Options.Create(new CacheGrainOptions { }),
                new NullLogger<CacheGrain>(),
                storage,
                Mock.Of<ISystemClock>(),
                Mock.Of<IGrainIdentity>(x => x.PrimaryKeyString == key),
                director,
                timers);

            // act - this will load the value from storage
            await grain.OnActivateAsync().ConfigureAwait(false);
            var result = await grain.GetAsync().ConfigureAwait(false);

            // assert
            Assert.Same(value, result.Value.Value);

            // arrange - clear storage
            Mock.Get(storage).Setup(x => x.ReadAsync(key, default)).Returns(Task.FromResult<CacheItem?>(null));

            // act - this must reuse the value in memory
            result = await grain.GetAsync().ConfigureAwait(false);

            // assert
            Assert.Same(value, result.Value.Value);
        }

        [Fact]
        public async Task RemoveClearsValueFromMemoryAndStorage()
        {
            // arrange
            var key = "SomeKey";
            var value = Guid.NewGuid().ToByteArray();
            var options = Options.Create(new CacheGrainOptions { });
            var logger = new NullLogger<CacheGrain>();
            var storage = new MemoryCacheStorage();
            var clock = Mock.Of<ISystemClock>();
            var identity = Mock.Of<IGrainIdentity>(x => x.PrimaryKeyString == key);

            var entry = Mock.Of<ICacheEntry>();
            Mock.Get(entry).Setup(x => x.Commit()).Returns(entry);
            var director = Mock.Of<ICacheDirector>(x => x.CreateEntry(key, value.Length + IntPtr.Size) == entry);

            var timers = Mock.Of<ITimerRegistry>();
            var context = Mock.Of<ICacheGrainContext>();

            var grain = new CacheGrain(context, options, logger, storage, clock, identity, director, timers);

            // act - set the value
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
            Assert.Same(value, result.Value.Value);

            // act - clear the value
            await grain.RemoveAsync().ConfigureAwait(false);

            // assert value is cleared from storage
            Assert.False((await storage.ReadAsync(key).ConfigureAwait(false)).HasValue);

            // arrange - add dummy value to storage
            await storage.WriteAsync(key, new CacheItem(value, absolute, sliding)).ConfigureAwait(false);

            // assert value is cleared from memory and not reloaded
            result = await grain.GetAsync().ConfigureAwait(false);
            Assert.Null(result.Value.Value);
        }
    }
}