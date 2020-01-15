using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Outkeep.Core.Caching;
using Outkeep.Core.Storage;
using Outkeep.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Grains.Tests
{
    [Collection(nameof(ClusterCollection))]
    public class CacheGrainTests
    {
        private readonly ClusterFixture _fixture;

        public CacheGrainTests(ClusterFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task GetReturnsNullValueFromStorage()
        {
            // arrange
            var key = Guid.NewGuid().ToString();

            // act
            var result = await _fixture.Cluster.GrainFactory
                .GetGrain<ICacheGrain>(key)
                .GetAsync();

            // assert
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task GetReturnsCorrectValueFromStorage()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();

            await _fixture.PrimarySiloServiceProvider.GetRequiredService<ICacheStorage>()
                .WriteAsync(key, new CacheItem(value, null, null))
                .ConfigureAwait(false);

            // act
            var result = await _fixture.Cluster.GrainFactory
                .GetGrain<ICacheGrain>(key)
                .GetAsync();

            // assert
            Assert.NotNull(result.Value.Value);
            Assert.Equal(value, result.Value.Value);
        }

        [Fact]
        public async Task GetReturnsCorrectValueFromMemory()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();

            await _fixture.PrimarySiloServiceProvider.GetRequiredService<ICacheStorage>()
                .WriteAsync(key, new CacheItem(value, null, null))
                .ConfigureAwait(false);

            // act - this will load the value from storage
            var result = await _fixture.Cluster.GrainFactory
                .GetGrain<ICacheGrain>(key)
                .GetAsync();

            // assert
            Assert.Equal(value, result.Value.Value);

            // arrange - clear storage
            await _fixture.PrimarySiloServiceProvider.GetRequiredService<ICacheStorage>()
                .ClearAsync(key)
                .ConfigureAwait(false);

            // act - this must reuse the value in memory
            result = await _fixture.Cluster.GrainFactory
                .GetGrain<ICacheGrain>(key)
                .GetAsync();

            // assert
            Assert.Equal(value, result.Value.Value);
        }

        [Fact]
        public async Task RemoveClearsValueFromMemoryAndStorage()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var grain = _fixture.Cluster.GrainFactory.GetGrain<ICacheGrain>(key);

            // act - set the value
            var absolute = DateTimeOffset.UtcNow.AddHours(1);
            var sliding = TimeSpan.FromMinutes(1);
            await grain.SetAsync(new Immutable<byte[]?>(value), absolute, sliding).ConfigureAwait(false);

            // assert value is set in storage
            var stored = await _fixture.PrimarySiloServiceProvider
                .GetRequiredService<ICacheStorage>()
                .ReadAsync(key)
                .ConfigureAwait(false);

            Assert.Equal(value, stored.Value.Value);
            Assert.Equal(absolute, stored.Value.AbsoluteExpiration);
            Assert.Equal(sliding, stored.Value.SlidingExpiration);

            // assert value is set in memory
            var result = await grain.GetAsync().ConfigureAwait(false);
            Assert.Equal(value, result.Value.Value);

            // act - clear the value
            await grain.RemoveAsync().ConfigureAwait(false);

            // assert value is cleared from storage
            var cleared = await _fixture.PrimarySiloServiceProvider
                .GetRequiredService<ICacheStorage>()
                .ReadAsync(key)
                .ConfigureAwait(false);

            Assert.False(cleared.HasValue);

            // arrange - add dummy value to storage
            await _fixture.PrimarySiloServiceProvider
                .GetRequiredService<ICacheStorage>()
                .WriteAsync(key, new CacheItem(value, absolute, sliding))
                .ConfigureAwait(false);

            // assert value is cleared from memory and not reloaded
            result = await grain.GetAsync().ConfigureAwait(false);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task DoesNotLoadExpiredItemFromStorage()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();

            await _fixture.PrimarySiloServiceProvider
                .GetRequiredService<ICacheStorage>()
                .WriteAsync(key, new CacheItem(value, DateTimeOffset.UtcNow.AddMinutes(-1), null))
                .ConfigureAwait(false);

            // act
            var result = await _fixture.Cluster.GrainFactory
                .GetCacheGrain(key)
                .GetAsync();

            // assert
            Assert.NotEqual(Guid.Empty, result.Tag);
            Assert.Null(result.Value.Value);

            // act
            var repeat = await _fixture.Cluster.GrainFactory
                .GetCacheGrain(key)
                .GetAsync();

            // assert
            Assert.Equal(result.Tag, repeat.Tag);
            Assert.Equal(result.Value, repeat.Value);
        }

        [Fact]
        public async Task HandlesExpiration()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(1);

            // act
            await _fixture.Cluster.GrainFactory
                .GetCacheGrain(key)
                .SetAsync(new Immutable<byte[]?>(value), absoluteExpiration, null)
                .ConfigureAwait(false);

            var result = await _fixture.Cluster.GrainFactory
                .GetCacheGrain(key)
                .GetAsync();

            // assert
            Assert.Equal(value, result.Value.Value);
            Assert.NotEqual(Guid.Empty, result.Tag);

            // wait for the cache director to expire the entry and complete the expired task
            await Task.Delay(2000).ConfigureAwait(false);

            // act
            result = await _fixture.Cluster.GrainFactory
                .GetCacheGrain(key)
                .GetAsync();

            // assert
            Assert.Null(result.Value.Value);
            Assert.NotEqual(Guid.Empty, result.Tag);

            // act
            var repeat = await _fixture.Cluster.GrainFactory
                .GetCacheGrain(key)
                .GetAsync();

            // assert
            Assert.Equal(result.Tag, repeat.Tag);
            Assert.Equal(result.Value, repeat.Value);
        }

        [Fact]
        public async Task RemoveNoopsIfEmpty()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var grain = _fixture.Cluster.GrainFactory.GetCacheGrain(key);

            // act
            await grain.RemoveAsync().ConfigureAwait(false);

            // assert
            Assert.True(true);
        }

        [Fact]
        public async Task SettingNullClearsStorage()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var grain = _fixture.Cluster.GrainFactory.GetCacheGrain(key);
            var storage = _fixture.PrimarySiloServiceProvider.GetService<ICacheStorage>();

            await storage.WriteAsync(key, new CacheItem(Guid.NewGuid().ToByteArray(), DateTimeOffset.UtcNow.AddDays(1), TimeSpan.MaxValue)).ConfigureAwait(false);

            // act
            await grain.SetAsync(new Immutable<byte[]?>(null), DateTimeOffset.UtcNow.AddMinutes(1), TimeSpan.MaxValue).ConfigureAwait(false);

            // assert
            var result = await storage.ReadAsync(key).ConfigureAwait(false);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task RefreshUpdatesLastAccessedTimestamp()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var grain = _fixture.Cluster.GrainFactory.GetCacheGrain(key);
            var director = _fixture.PrimarySiloServiceProvider.GetRequiredService<ICacheDirector>();

            // act
            await grain.SetAsync(Guid.NewGuid().ToByteArray().AsNullableImmutable(), null, null).ConfigureAwait(false);

            // assert
            Assert.True(director.TryGetEntry(key, out var entry));
            var accessed = entry?.UtcLastAccessed;

            // act
            await grain.RefreshAsync().ConfigureAwait(false);

            // assert
            Assert.True(entry?.UtcLastAccessed > accessed);
        }

        [Fact]
        public async Task PollAsyncLifecycle()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var tag = Guid.NewGuid();
            var grain = _fixture.Cluster.GrainFactory.GetCacheGrain(key);
            var options = _fixture.PrimarySiloServiceProvider.GetRequiredService<IOptions<CacheGrainOptions>>().Value;

            // act - this returns the current pulse with no delay
            var watch1 = Stopwatch.StartNew();
            var result1 = await grain.PollAsync(tag).ConfigureAwait(false);
            watch1.Stop();

            // assert
            Assert.True(watch1.Elapsed < TimeSpan.FromSeconds(1));
            Assert.NotEqual(Guid.Empty, result1.Tag);
            Assert.Null(result1.Value.Value);

            // act - this waits till timeout to return an empty pulse with the same tag as input
            var watch2 = Stopwatch.StartNew();
            var result2 = await grain.PollAsync(result1.Tag).ConfigureAwait(false);
            watch2.Stop();

            // assert
            Assert.True(watch2.Elapsed > options.ReactivePollingTimeout);
            Assert.Equal(result1.Tag, result2.Tag);
            Assert.Null(result2.Value.Value);

            // act - this issues another long-poll but does not wait
            var watch3 = Stopwatch.StartNew();
            var task3 = grain.PollAsync(result2.Tag);

            // act - allow the long poll to start
            await Task.Delay(100).ConfigureAwait(false);

            // act - change the value to resolve the poll
            var value3 = Guid.NewGuid().ToByteArray();
            await grain.SetAsync(value3.AsNullableImmutable(), null, null).ConfigureAwait(false);

            // assert
            var result3 = await task3.ConfigureAwait(false);
            watch3.Stop();
            Assert.True(watch3.Elapsed < TimeSpan.FromSeconds(1));
            Assert.NotEqual(Guid.Empty, result3.Tag);
            Assert.NotEqual(result2.Tag, result3.Tag);
            Assert.Equal(value3, result3.Value.Value);
        }
    }
}