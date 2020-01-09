using Outkeep.Core.Storage;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class NullCacheStorageTests
    {
        [Fact]
        public async Task Cycles()
        {
            // arrange
            var service = new NullCacheStorage();
            var key = "SomeKey";
            var value = Guid.NewGuid().ToByteArray();

            // act
            await service.WriteAsync(key, new CacheItem(value, null, null)).ConfigureAwait(false);
            var result = await service.ReadAsync(key).ConfigureAwait(false);
            await service.ClearAsync(key).ConfigureAwait(false);

            // assert
            Assert.Null(result);
        }
    }
}