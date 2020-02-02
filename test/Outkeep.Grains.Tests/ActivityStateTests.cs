using Outkeep.Governance;
using System;
using System.Collections.Generic;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class ActivityStateTests
    {
        [Fact]
        public void OperatorsReturnExpectedResults()
        {
            ActivityState? left;
            ActivityState? right;

            // assert null vs null
            left = null;
            right = null;
            Assert.True(Equals(left, right));
            Assert.True(EqualityComparer<ActivityState>.Default.Equals(left, right));
            Assert.Equal(0, Comparer<ActivityState>.Default.Compare(left, right));
            Assert.True(left! == right!);
            Assert.False(left! != right!);
            Assert.False(left! < right!);
            Assert.False(left! > right!);
            Assert.True(left! <= right!);
            Assert.True(left! >= right!);

            // assert null vs value
            left = null;
            right = new ActivityState { Priority = ActivityPriority.Normal };
            Assert.False(Equals(left, right));
            Assert.False(EqualityComparer<ActivityState>.Default.Equals(left, right));
            Assert.Equal(-1, Comparer<ActivityState>.Default.Compare(left, right));
            Assert.False(left! == right!);
            Assert.True(left! != right!);
            Assert.True(left! < right!);
            Assert.False(left! > right!);
            Assert.True(left! <= right!);
            Assert.False(left! >= right!);

            // assert value vs null
            left = new ActivityState { Priority = ActivityPriority.Normal };
            right = null;
            Assert.False(Equals(left, right));
            Assert.False(EqualityComparer<ActivityState>.Default.Equals(left, right));
            Assert.Equal(1, Comparer<ActivityState>.Default.Compare(left, right));
            Assert.False(left! == right!);
            Assert.True(left! != right!);
            Assert.False(left! < right!);
            Assert.True(left! > right!);
            Assert.False(left! <= right!);
            Assert.True(left! >= right!);

            // assert low value vs high value
            left = new ActivityState { Priority = ActivityPriority.Low };
            right = new ActivityState { Priority = ActivityPriority.High };
            Assert.False(Equals(left, right));
            Assert.False(EqualityComparer<ActivityState>.Default.Equals(left, right));
            Assert.Equal(-1, Comparer<ActivityState>.Default.Compare(left, right));
            Assert.False(left! == right!);
            Assert.True(left! != right!);
            Assert.True(left! < right!);
            Assert.False(left! > right!);
            Assert.True(left! <= right!);
            Assert.False(left! >= right!);

            // assert high value vs low value
            left = new ActivityState { Priority = ActivityPriority.High };
            right = new ActivityState { Priority = ActivityPriority.Low };
            Assert.False(Equals(left, right));
            Assert.False(EqualityComparer<ActivityState>.Default.Equals(left, right));
            Assert.Equal(1, Comparer<ActivityState>.Default.Compare(left, right));
            Assert.False(left! == right!);
            Assert.True(left! != right!);
            Assert.False(left! < right!);
            Assert.True(left! > right!);
            Assert.False(left! <= right!);
            Assert.True(left! >= right!);

            // assert normal value vs normal value
            left = new ActivityState { Priority = ActivityPriority.Normal };
            right = new ActivityState { Priority = ActivityPriority.Normal };
            Assert.True(Equals(left, right));
            Assert.True(EqualityComparer<ActivityState>.Default.Equals(left, right));
            Assert.Equal(0, Comparer<ActivityState>.Default.Compare(left, right));
            Assert.True(left! == right!);
            Assert.False(left! != right!);
            Assert.False(left! < right!);
            Assert.False(left! > right!);
            Assert.True(left! <= right!);
            Assert.True(left! >= right!);

            // assert value vs itself
            left = new ActivityState { Priority = ActivityPriority.Normal };
            right = left;
            Assert.True(Equals(left, right));
            Assert.True(EqualityComparer<ActivityState>.Default.Equals(left, right));
            Assert.Equal(0, Comparer<ActivityState>.Default.Compare(left, right));
            Assert.True(left! == right!);
            Assert.False(left! != right!);
            Assert.False(left! < right!);
            Assert.False(left! > right!);
            Assert.True(left! <= right!);
            Assert.True(left! >= right!);

            // assert ordering
            var items = new ActivityState?[]
            {
                null,
                new ActivityState { Priority = ActivityPriority.Normal },
                new ActivityState { Priority = ActivityPriority.High, },
                new ActivityState { Priority = ActivityPriority.Low, },
                new ActivityState { Priority = ActivityPriority.High },
                new ActivityState { Priority = ActivityPriority.Low },
                new ActivityState { Priority = ActivityPriority.Normal },
                null
            };
            Array.Sort(items);

            Assert.Collection(items,
                x => Assert.Null(x),
                x => Assert.Null(x),
                x => Assert.Equal(ActivityPriority.Low, x?.Priority),
                x => Assert.Equal(ActivityPriority.Low, x?.Priority),
                x => Assert.Equal(ActivityPriority.Normal, x?.Priority),
                x => Assert.Equal(ActivityPriority.Normal, x?.Priority),
                x => Assert.Equal(ActivityPriority.High, x?.Priority),
                x => Assert.Equal(ActivityPriority.High, x?.Priority));
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