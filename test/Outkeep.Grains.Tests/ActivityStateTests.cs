using Outkeep.Governance;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class ActivityStateTests
    {
        [Fact]
        [SuppressMessage("Major Bug", "S1764:Identical expressions should not be used on both sides of a binary operator")]
        public void EqualsSameObject()
        {
            // arrange
            var state = new ActivityState
            {
                Priority = ActivityPriority.Normal
            };

            // act
            var result = state.Equals(state);

            // assert
            Assert.True(result);
        }

        [Fact]
        public void DoesNotEqualNull()
        {
            // arrange
            var state = new ActivityState
            {
                Priority = ActivityPriority.Normal
            };

            // act
            var result = state.Equals(null!);

            // assert
            Assert.False(result);
        }

        [Fact]
        public void EqualsSameValue()
        {
            // arrange
            var left = new ActivityState
            {
                Priority = ActivityPriority.Normal
            };
            var right = new ActivityState
            {
                Priority = ActivityPriority.Normal
            };

            // act
            var result = left.Equals(right);

            // assert
            Assert.True(result);
        }

        [Fact]
        public void DoesNotEqualDifferentValue()
        {
            // arrange
            var low = new ActivityState
            {
                Priority = ActivityPriority.Low
            };
            var normal = new ActivityState
            {
                Priority = ActivityPriority.Normal
            };
            var high = new ActivityState
            {
                Priority = ActivityPriority.High
            };

            // act and assert
            Assert.False(low.Equals(normal));
            Assert.False(low.Equals(high));
            Assert.False(normal.Equals(high));
        }

        [Fact]
        public void GetHashCodeReturnsHashCode()
        {
            // arrange
            var state = new ActivityState
            {
                Priority = ActivityPriority.Normal
            };

            // act
            var result = state.GetHashCode();

            // assert
            Assert.Equal(HashCode.Combine(state.Priority), result);
        }
    }
}