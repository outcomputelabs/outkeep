using Microsoft.Extensions.DependencyInjection;
using Orleans.Concurrency;
using Outkeep.Core.Storage;
using Outkeep.Interfaces;
using System;
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
    }
}