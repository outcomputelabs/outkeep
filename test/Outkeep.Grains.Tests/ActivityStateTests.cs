using Outkeep.Governance;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class ActivityStateTests
    {
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