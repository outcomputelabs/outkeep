using Outkeep.Core;
using System;
using Xunit;

namespace Outkeep.Interfaces.Tests
{
    public class DefaultSystemClockTests
    {
        [Fact]
        public void ReturnsUtcNow()
        {
            var clock = new DefaultSystemClock();

            var result = clock.UtcNow;
            var now = DateTimeOffset.UtcNow;

            Assert.InRange(result, now.AddMilliseconds(-100), now.AddMilliseconds(100));
        }
    }
}