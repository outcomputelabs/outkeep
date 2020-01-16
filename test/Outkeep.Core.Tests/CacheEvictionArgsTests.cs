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
            var cause = EvictionCause.Expired;

            // act
            var args = new CacheEvictionArgs<string>(entry, cause);

            // assert
            Assert.Same(entry, args.CacheEntry);
            Assert.Equal(cause, args.EvictionCause);
        }

        [Fact]
        public void TypedEquals()
        {
            // arrange
            var entry1 = Mock.Of<ICacheEntry<string>>();
            var entry2 = Mock.Of<ICacheEntry<string>>();
            var args1 = new CacheEvictionArgs<string>(entry1, EvictionCause.Replaced);
            var args2 = new CacheEvictionArgs<string>(entry1, EvictionCause.Replaced);
            var args3 = new CacheEvictionArgs<string>(entry1, EvictionCause.Removed);
            var args4 = new CacheEvictionArgs<string>(entry2, EvictionCause.Replaced);

            // act
            var result1 = EqualityComparer<CacheEvictionArgs<string>>.Default.Equals(args1, args2);
            var result2 = EqualityComparer<CacheEvictionArgs<string>>.Default.Equals(args1, args3);
            var result3 = EqualityComparer<CacheEvictionArgs<string>>.Default.Equals(args1, args4);

            // assert
            Assert.True(result1);
            Assert.False(result2);
            Assert.False(result3);
        }

        [Fact]
        public void ObjectEquals()
        {
            // arrange
            var entry1 = Mock.Of<ICacheEntry<string>>();
            var entry2 = Mock.Of<ICacheEntry<string>>();
            var args1 = new CacheEvictionArgs<string>(entry1, EvictionCause.Replaced);
            var args2 = new CacheEvictionArgs<string>(entry1, EvictionCause.Replaced);
            var args3 = new CacheEvictionArgs<string>(entry1, EvictionCause.Removed);
            var args4 = new CacheEvictionArgs<string>(entry2, EvictionCause.Replaced);

            // act
            var result1 = Equals(args1, args2);
            var result2 = Equals(args1, args3);
            var result3 = Equals(args1, args4);
            var result4 = Equals(args1, null);
            var result5 = Equals(args1, new object());

            // assert
            Assert.True(result1);
            Assert.False(result2);
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
            var args1 = new CacheEvictionArgs<string>(entry1, EvictionCause.Replaced);
            var args2 = new CacheEvictionArgs<string>(entry1, EvictionCause.Replaced);
            var args3 = new CacheEvictionArgs<string>(entry1, EvictionCause.Removed);
            var args4 = new CacheEvictionArgs<string>(entry2, EvictionCause.Replaced);

            // act
            var result1 = args1 == args2;
            var result2 = args1 == args3;
            var result3 = args1 == args4;

            // assert
            Assert.True(result1);
            Assert.False(result2);
            Assert.False(result3);
        }

        [Fact]
        public void OperatorNotEquals()
        {
            // arrange
            var entry1 = Mock.Of<ICacheEntry<string>>();
            var entry2 = Mock.Of<ICacheEntry<string>>();
            var args1 = new CacheEvictionArgs<string>(entry1, EvictionCause.Replaced);
            var args2 = new CacheEvictionArgs<string>(entry1, EvictionCause.Replaced);
            var args3 = new CacheEvictionArgs<string>(entry1, EvictionCause.Removed);
            var args4 = new CacheEvictionArgs<string>(entry2, EvictionCause.Replaced);

            // act
            var result1 = args1 != args2;
            var result2 = args1 != args3;
            var result3 = args1 != args4;

            // assert
            Assert.False(result1);
            Assert.True(result2);
            Assert.True(result3);
        }

        [Fact]
        public void GetsHashCode()
        {
            // arrange
            var entry = Mock.Of<ICacheEntry<string>>(x => x.GetHashCode() == 123);
            var cause = EvictionCause.Replaced;
            var expected = HashCode.Combine(entry, cause);
            var args = new CacheEvictionArgs<string>(entry, cause);

            // act
            var result = args.GetHashCode();

            // assert
            Assert.Equal(expected, result);
        }
    }
}