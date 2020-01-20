using Moq;
using Outkeep.Core.Caching;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class CacheEntryExtensionsTests
    {
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
    }
}