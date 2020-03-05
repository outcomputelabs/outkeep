using Outkeep.Time;
using System;
using Xunit;

namespace Outkeep.Interfaces.Tests
{
    public class SystemClockTests
    {
        [Fact]
        public void ReturnsUtcNow()
        {
            var clock = new SystemClock();
            var result = clock.UtcNow;
            var now = DateTimeOffset.UtcNow;

            Assert.InRange(result, now.AddMilliseconds(-100), now.AddMilliseconds(100));
        }
    }
}