using Moq;
using Outkeep.Core.Caching;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class CacheEntryExtensionsTests
    {
        [Fact]
        public void SetAbsoluteExpiration()
        {
            // arrange
            var entry = Mock.Of<ICacheEntry>();
            var absoluteExpiration = DateTimeOffset.UtcNow;

            // act
            var result = entry.SetAbsoluteExpiration(absoluteExpiration);

            // assert
            Mock.Get(entry).VerifySet(x => x.AbsoluteExpiration = absoluteExpiration);
            Assert.Same(entry, result);
        }

        [Fact]
        public void SetSlidingExpiration()
        {
            // arrange
            var entry = Mock.Of<ICacheEntry>();
            var slidingExpiration = TimeSpan.FromMinutes(1);

            // act
            var result = entry.SetSlidingExpiration(slidingExpiration);

            // assert
            Mock.Get(entry).VerifySet(x => x.SlidingExpiration = slidingExpiration);
            Assert.Same(entry, result);
        }
    }
}