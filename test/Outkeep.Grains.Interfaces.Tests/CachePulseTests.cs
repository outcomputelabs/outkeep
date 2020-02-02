using Outkeep.Caching;
using System;
using System.Collections.Generic;
using Xunit;

namespace Outkeep.Grains.Interfaces.Tests
{
    public class CachePulseTests
    {
        [Fact]
        public void Defaults()
        {
            // act
            var pulse = new CachePulse();

            // assert
            Assert.Equal(Guid.Empty, pulse.Tag);
            Assert.Null(pulse.Value);
        }

        [Fact]
        public void Values()
        {
            // arrange
            var tag = Guid.NewGuid();
            var value = Guid.NewGuid().ToByteArray();

            // act
            var pulse = new CachePulse(tag, value);

            // assert
            Assert.Equal(tag, pulse.Tag);
            Assert.Equal(value, pulse.Value);
        }

        [Fact]
        public void OperatorsWork()
        {
            // arrange
            CachePulse left;
            CachePulse right;

            // assert empty vs empty
            left = new CachePulse();
            right = new CachePulse();
            Assert.True(Equals(left, right));
            Assert.True(EqualityComparer<CachePulse>.Default.Equals(left, right));
            Assert.True(left == right);
            Assert.False(left != right);

            // assert empty vs random
            left = new CachePulse();
            right = new CachePulse(Guid.NewGuid(), Guid.NewGuid().ToByteArray());
            Assert.False(Equals(left, right));
            Assert.False(EqualityComparer<CachePulse>.Default.Equals(left, right));
            Assert.False(left == right);
            Assert.True(left != right);

            // assert random vs empty
            left = new CachePulse(Guid.NewGuid(), Guid.NewGuid().ToByteArray());
            right = new CachePulse();
            Assert.False(Equals(left, right));
            Assert.False(EqualityComparer<CachePulse>.Default.Equals(left, right));
            Assert.False(left == right);
            Assert.True(left != right);

            // assert random vs random
            left = new CachePulse(Guid.NewGuid(), Guid.NewGuid().ToByteArray());
            right = new CachePulse(Guid.NewGuid(), Guid.NewGuid().ToByteArray());
            Assert.False(Equals(left, right));
            Assert.False(EqualityComparer<CachePulse>.Default.Equals(left, right));
            Assert.False(left == right);
            Assert.True(left != right);

            // assert random vs same
            left = new CachePulse(Guid.NewGuid(), Guid.NewGuid().ToByteArray());
            right = left;
            Assert.True(Equals(left, right));
            Assert.True(EqualityComparer<CachePulse>.Default.Equals(left, right));
            Assert.True(left == right);
            Assert.False(left != right);
        }
    }
}