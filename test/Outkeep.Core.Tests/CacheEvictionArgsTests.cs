using Moq;
using Outkeep.Core.Caching;
using System;
using System.Collections.Generic;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class CacheEvictionArgsTests
    {
        [Fact]
        public void Properties()
        {
            // arrange
            var entry = Mock.Of<ICacheEntry<string>>();

            // act
            var args = new CacheEvictionArgs<string>(entry);

            // assert
            Assert.Same(entry, args.CacheEntry);
        }

        [Fact]
        public void TypedEquals()
        {
            // arrange
            var entry1 = Mock.Of<ICacheEntry<string>>();
            var entry2 = Mock.Of<ICacheEntry<string>>();
            var args1 = new CacheEvictionArgs<string>(entry1);
            var args2 = new CacheEvictionArgs<string>(entry1);
            var args4 = new CacheEvictionArgs<string>(entry2);

            // act
            var result1 = EqualityComparer<CacheEvictionArgs<string>>.Default.Equals(args1, args2);
            var result3 = EqualityComparer<CacheEvictionArgs<string>>.Default.Equals(args1, args4);

            // assert
            Assert.True(result1);
            Assert.False(result3);
        }

        [Fact]
        public void ObjectEquals()
        {
            // arrange
            var entry1 = Mock.Of<ICacheEntry<string>>();
            var entry2 = Mock.Of<ICacheEntry<string>>();
            var args1 = new CacheEvictionArgs<string>(entry1);
            var args2 = new CacheEvictionArgs<string>(entry1);
            var args4 = new CacheEvictionArgs<string>(entry2);

            // act
            var result1 = Equals(args1, args2);
            var result3 = Equals(args1, args4);
            var result4 = Equals(args1, null);
            var result5 = Equals(args1, new object());

            // assert
            Assert.True(result1);
            Assert.False(result3);
            Assert.False(result4);
            Assert.False(result5);
        }

        [Fact]
        public void OperatorEquals()
        {
            // arrange
            var entry1 = Mock.Of<ICacheEntry<string>>();
            var entry2 = Mock.Of<ICacheEntry<string>>();
            var args1 = new CacheEvictionArgs<string>(entry1);
            var args2 = new CacheEvictionArgs<string>(entry1);
            var args4 = new CacheEvictionArgs<string>(entry2);

            // act
            var result1 = args1 == args2;
            var result3 = args1 == args4;

            // assert
            Assert.True(result1);
            Assert.False(result3);
        }

        [Fact]
        public void OperatorNotEquals()
        {
            // arrange
            var entry1 = Mock.Of<ICacheEntry<string>>();
            var entry2 = Mock.Of<ICacheEntry<string>>();
            var args1 = new CacheEvictionArgs<string>(entry1);
            var args2 = new CacheEvictionArgs<string>(entry1);
            var args4 = new CacheEvictionArgs<string>(entry2);

            // act
            var result1 = args1 != args2;
            var result3 = args1 != args4;

            // assert
            Assert.False(result1);
            Assert.True(result3);
        }

        [Fact]
        public void GetsHashCode()
        {
            // arrange
            var entry = Mock.Of<ICacheEntry<string>>(x => x.GetHashCode() == 123);
            var expected = HashCode.Combine(entry);
            var args = new CacheEvictionArgs<string>(entry);

            // act
            var result = args.GetHashCode();

            // assert
            Assert.Equal(expected, result);
        }
    }
}