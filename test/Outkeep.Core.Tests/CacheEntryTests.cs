using Moq;
using Outkeep.Core.Caching;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class CacheEntryTests
    {
        [Fact]
        public void Cycles()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var size = 100;
            var context = Mock.Of<ICacheContext>();

            // act
            var entry = new CacheEntry(key, size, context);

            // assert
            Assert.Equal(size, entry.Size);
            Assert.Equal(key, entry.Key);

            // act
            entry.Commit();

            // assert
            Mock.Get(context).Verify(x => x.OnEntryCommitted(entry));

            // act
            entry.Dispose();

            // assert
            Mock.Get(context).Verify(x => x.OnEntryExpired(entry));
        }

        [Fact]
        public async Task EvictionSetsTask()
        {
            // arrange
            var context = Mock.Of<ICacheContext>();

            // act
            using var entry = new CacheEntry("SomeKey", 1000, context);

            // assert
            Assert.Equal(TaskStatus.WaitingForActivation, entry.Evicted.Status);

            // act
            entry.SetEvicted();

            // assert
            await entry.Evicted.ConfigureAwait(false);
            Assert.Equal(TaskStatus.RanToCompletion, entry.Evicted.Status);
        }
    }
}