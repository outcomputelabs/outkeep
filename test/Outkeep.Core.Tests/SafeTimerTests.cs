using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class SafeTimerTests
    {
        [Fact]
        public void ThrowsOnNullLogger()
        {
            // act
            Assert.Throws<ArgumentNullException>("logger", () =>
            {
                // act
                new SafeTimer(null!, null!, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
        }
    }
}