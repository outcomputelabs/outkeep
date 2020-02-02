using Outkeep.Governance;
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
    }
}