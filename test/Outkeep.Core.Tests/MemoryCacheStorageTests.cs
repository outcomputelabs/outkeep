using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class MemoryCacheStorageTests
    {
        [Fact]
        public async Task Cycles()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow.AddHours(1);
            var slidingExpiration = TimeSpan.FromMinutes(1);
            var storage = new MemoryCacheStorage();

            // attempt to read empty state
            var result = await storage.ReadAsync(key).ConfigureAwait(false);
            Assert.False(result.HasValue);

            // write some state
            await storage.WriteAsync(key, new CacheItem(value, absoluteExpiration, slidingExpiration)).ConfigureAwait(false);

            // attempt to read that state
            result = await storage.ReadAsync(key).ConfigureAwait(false);
            Assert.True(result.HasValue);
            Assert.Same(value, result.Value.Value);
            Assert.Equal(absoluteExpiration, result.Value.AbsoluteExpiration);
            Assert.Equal(slidingExpiration, result.Value.SlidingExpiration);

            // clear the state
            await storage.ClearAsync(key).ConfigureAwait(false);

            // attempt to read the cleared state
            result = await storage.ReadAsync(key).ConfigureAwait(false);
            Assert.False(result.HasValue);
        }
    }
}