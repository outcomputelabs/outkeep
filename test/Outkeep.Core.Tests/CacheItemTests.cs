using Outkeep.Core.Storage;
using System;
using System.Linq;
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
            var result = item.Equals(null!);

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

        [Fact]
        public void EqualsOtherItem()
        {
            // arrange
            var value = Guid.NewGuid().ToByteArray();
            var absolute = DateTimeOffset.UtcNow;
            var sliding = TimeSpan.MaxValue;
            var item = new CacheItem(value, absolute, sliding);
            var other = new CacheItem(value, absolute, sliding);

            // act
            var result = item.Equals(other);

            // assert
            Assert.True(result);
        }

        [Fact]
        public void DoesNotEqualsOtherItemWithDifferentArray()
        {
            // arrange
            var value = Guid.NewGuid().ToByteArray();
            var absolute = DateTimeOffset.UtcNow;
            var sliding = TimeSpan.MaxValue;
            var item = new CacheItem(value.ToArray(), absolute, sliding);
            var other = new CacheItem(value.ToArray(), absolute, sliding);

            // act
            var result = item.Equals(other);

            // assert
            Assert.False(result);
        }

        [Fact]
        public void DoesNotEqualsOtherItemWithDifferentAbsoluteExpiration()
        {
            // arrange
            var value = Guid.NewGuid().ToByteArray();
            var sliding = TimeSpan.MaxValue;
            var item = new CacheItem(value, DateTimeOffset.UtcNow.AddDays(1), sliding);
            var other = new CacheItem(value, DateTimeOffset.UtcNow.AddDays(2), sliding);

            // act
            var result = item.Equals(other);

            // assert
            Assert.False(result);
        }

        [Fact]
        public void DoesNotEqualsOtherItemWithDifferentSlidingExpiration()
        {
            // arrange
            var value = Guid.NewGuid().ToByteArray();
            var absolute = DateTimeOffset.UtcNow;
            var item = new CacheItem(value, absolute, TimeSpan.MinValue);
            var other = new CacheItem(value, absolute, TimeSpan.MaxValue);

            // act
            var result = item.Equals(other);

            // assert
            Assert.False(result);
        }

        [Fact]
        public void GetsHashCode()
        {
            // arrange
            var value = Guid.NewGuid().ToByteArray();
            var absolute = DateTimeOffset.UtcNow;
            var sliding = TimeSpan.MaxValue;
            var item = new CacheItem(value, absolute, sliding);
            var hash = HashCode.Combine(value, absolute, sliding);

            // act
            var result = item.GetHashCode();

            // assert
            Assert.Equal(hash, result);
        }

        [Fact]
        public void OperatorEqualsOtherItem()
        {
            // arrange
            var value = Guid.NewGuid().ToByteArray();
            var absolute = DateTimeOffset.UtcNow;
            var sliding = TimeSpan.MaxValue;
            var item = new CacheItem(value, absolute, sliding);
            var other = new CacheItem(value, absolute, sliding);

            // act
            var result = item == other;

            // assert
            Assert.True(result);
        }

        [Fact]
        public void OperatorNotEqualsOtherItem()
        {
            // arrange
            var value = Guid.NewGuid().ToByteArray();
            var absolute = DateTimeOffset.UtcNow;
            var sliding = TimeSpan.MaxValue;
            var item = new CacheItem(value.ToArray(), absolute, sliding);
            var other = new CacheItem(value.ToArray(), absolute, sliding);

            // act
            var result = item != other;

            // assert
            Assert.True(result);
        }
    }
}