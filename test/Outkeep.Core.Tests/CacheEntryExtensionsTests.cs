using Moq;
using Outkeep.Core.Caching;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class CacheEntryExtensionsTests
    {
        [Fact]
        public void SetAbsoluteExpiration()
        {
            // arrange
            var entry = Mock.Of<ICacheEntry<string>>();
            var absoluteExpiration = DateTimeOffset.UtcNow;

            // act
            var result = entry.SetAbsoluteExpiration(absoluteExpiration);

            // assert
            Mock.Get(entry).VerifySet(x => x.AbsoluteExpiration = absoluteExpiration);
            Assert.Same(entry, result);
        }

        [Fact]
        public void SetAbsoluteExpirationThrowsOnNullEntry()
        {
            // arrange
            ICacheEntry<string> entry = null!;

            // act
            void action() => entry.SetAbsoluteExpiration(DateTimeOffset.UtcNow);

            // assert
            Assert.Throws<ArgumentNullException>(nameof(entry), action);
        }

        [Fact]
        public void SetPriority()
        {
            // arrange
            var entry = Mock.Of<ICacheEntry<string>>();
            var priority = CachePriority.Normal;

            // act
            var result = entry.SetPriority(priority);

            // assert
            Mock.Get(entry).VerifySet(x => x.Priority = priority);
            Assert.Same(entry, result);
        }

        [Fact]
        public void SetPriorityThrowsOnNullEntry()
        {
            // arrange
            ICacheEntry<string> entry = null!;

            // act
            void action() => entry.SetPriority(CachePriority.Normal);

            // assert
            Assert.Throws<ArgumentNullException>(nameof(entry), action);
        }

        [Fact]
        public async Task ContinueWithOnEvicted()
        {
            // arrange
            var entry = Mock.Of<ICacheEntry<string>>();
            var result = Task.FromResult(new CacheEvictionArgs<string>(entry));
            Mock.Get(entry).SetupGet(x => x.Evicted).Returns(result);

            // act
            var completed = false;
            Task action(CacheEvictionArgs<string> args)
            {
                completed = true;
                return Task.CompletedTask;
            }
            var chain = entry.ContinueWithOnEvicted(action, CancellationToken.None);

            // assert
            await Task.Delay(100).ConfigureAwait(false);
            Assert.Same(entry, chain);
            Assert.True(completed);
        }

        [Fact]
        public void ContinueWithOnEvictedThrowsOnNullEntry()
        {
            // arrange
            ICacheEntry<string> entry = null!;

            // act
            void action() => entry.ContinueWithOnEvicted(args => Task.CompletedTask, CancellationToken.None);

            // assert
            Assert.Throws<ArgumentNullException>(nameof(entry), action);
        }
    }
}