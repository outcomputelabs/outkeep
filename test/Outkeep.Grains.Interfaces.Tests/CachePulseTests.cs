using Outkeep.Caching;
using System;
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
    }
}