using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class ConcurrentDictionaryExtensionsTests
    {
        [Fact]
        public void TryRemoveRemovesMatchingEntry()
        {
            // arrange
            var dictionary = new ConcurrentDictionary<Guid, Guid>();
            var first = Guid.NewGuid();
            var second = Guid.NewGuid();
            dictionary[first] = first;
            dictionary[second] = second;

            // act
            var result = dictionary.TryRemove(new KeyValuePair<Guid, Guid>(first, first));

            // assert
            Assert.True(result);
            Assert.Single(dictionary);
            Assert.True(dictionary.ContainsKey(second));
        }

        [Fact]
        public void TryRemoveDoesNotRemovesNonMatchingEntry()
        {
            // arrange
            var dictionary = new ConcurrentDictionary<Guid, Guid>();
            var first = Guid.NewGuid();
            var second = Guid.NewGuid();
            dictionary[first] = first;
            dictionary[second] = second;

            // act
            var result = dictionary.TryRemove(new KeyValuePair<Guid, Guid>(first, second));

            // assert
            Assert.False(result);
            Assert.Equal(2, dictionary.Count);
            Assert.True(dictionary.ContainsKey(first));
            Assert.True(dictionary.ContainsKey(second));
        }

        [Fact]
        public void TryRemoveThrowsOnNullDictionary()
        {
            // arrange
            ConcurrentDictionary<Guid, Guid>? dictionary = null;

            // act
            void action() { dictionary!.TryRemove(new KeyValuePair<Guid, Guid>(Guid.Empty, Guid.Empty)); }

            // assert
            Assert.Throws<ArgumentNullException>(nameof(dictionary), action);
        }
    }
}