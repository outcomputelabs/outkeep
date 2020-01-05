using Moq;
using Outkeep.Core.Caching;
using System;
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
    }
}