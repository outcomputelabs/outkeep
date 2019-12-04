using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class CacheItemTests
    {
        [Fact]
        public void Constructs()
        {
            // arrange
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = new DateTimeOffset(1, 2, 3, 4, 5, 6, TimeSpan.Zero);
            var slidingExpiration = new TimeSpan(1, 2, 3);

            // act
            var item = new CacheItem(value, absoluteExpiration, slidingExpiration);

            // assert
            Assert.Same(value, item.Value);
            Assert.Equal(absoluteExpiration, item.AbsoluteExpiration);
            Assert.Equal(slidingExpiration, item.SlidingExpiration);
        }

        [Fact]
        public void DoesNotEqualNull()
        {
            // arrange
            var item = new CacheItem();

            // act
            var result = item.Equals(null);

            // assert
            Assert.False(result);
        }

        [Fact]
        public void DoesNotEqualOtherType()
        {
            // arrange
            var item = new CacheItem();

            // act
            var result = item.Equals(new object());

            // assert
            Assert.False(result);
        }
    }
}