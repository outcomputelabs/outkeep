using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Storage;
using Outkeep.Caching;
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
        public async Task DoesNotLoadExpiredItemFromStorage()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var grain = _fixture.PrimarySiloServiceProvider.GetRequiredService<IGrainFactory>().GetCacheGrain(key);
            var storage = _fixture.PrimarySiloServiceProvider.GetRequiredServiceByName<IGrainStorage>(OutkeepProviderNames.OutkeepCache);
            var reference = (GrainReference)grain;
            var grainType = $"{typeof(CacheGrain).FullName},{typeof(CacheGrain).Namespace}.State";

            var state = new GrainState<CacheGrainState>(new CacheGrainState
            {
                Tag = Guid.NewGuid(),
                Value = Guid.NewGuid().ToByteArray(),
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(-1),
                SlidingExpiration = null
            });
            await storage.WriteStateAsync(grainType, reference, state).ConfigureAwait(false);

            // act
            var result = await grain.GetAsync();

            // assert
            Assert.Equal(Guid.Empty, result.Tag);
            Assert.Null(result.Value.Value);

            // act
            var repeat = await grain.GetAsync();

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
            var grain = _fixture.Cluster.GrainFactory.GetCacheGrain(key);

            // act
            await grain
                .SetAsync(new Immutable<byte[]?>(value), absoluteExpiration, null)
                .ConfigureAwait(false);

            var result = await grain.GetAsync();

            // assert
            Assert.Equal(value, result.Value.Value);
            Assert.NotEqual(Guid.Empty, result.Tag);

            // wait for the cache director to expire the entry and complete the expired task
            await Task.Delay(2000).ConfigureAwait(false);

            // act
            result = await grain.GetAsync();

            // assert
            Assert.Equal(Guid.Empty, result.Tag);
            Assert.Null(result.Value.Value);

            // act
            var repeat = await grain.GetAsync();

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

            // assert - the initial pulse should be empty
            Assert.True(watch1.Elapsed < TimeSpan.FromSeconds(1));
            Assert.Equal(Guid.Empty, result1.Tag);
            Assert.Null(result1.Value.Value);

            // act - this waits till timeout to return an empty pulse with the same tag as input
            var watch2 = Stopwatch.StartNew();
            var result2 = await grain.PollAsync(result1.Tag).ConfigureAwait(false);
            watch2.Stop();

            // assert - the delayed pulse return the same tag and no value
            Assert.True(watch2.Elapsed >= options.ReactivePollingTimeout);
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

            // assert - the resolved returns the data we set midway
            var result3 = await task3.ConfigureAwait(false);
            watch3.Stop();
            Assert.True(watch3.Elapsed < TimeSpan.FromSeconds(1));
            Assert.NotEqual(Guid.Empty, result3.Tag);
            Assert.NotEqual(result2.Tag, result3.Tag);
            Assert.Equal(value3, result3.Value.Value);

            // act - access the value again to update the accessed timestamp
            await grain.PollAsync(Guid.NewGuid()).ConfigureAwait(false);

            // act - long poll the value again to test the empty pulse response
            var result5 = await grain.PollAsync(result3.Tag);
            Assert.Equal(result3.Tag, result5.Tag);
            Assert.Null(result5.Value.Value);
        }
    }
}