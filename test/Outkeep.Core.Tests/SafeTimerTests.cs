using Microsoft.Extensions.Logging.Abstractions;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class SafeTimerTests
    {
        [Fact]
        public void ThrowsOnNullLogger()
        {
            // assert
            Assert.Throws<ArgumentNullException>("logger", () =>
            {
                // act
                new SafeTimer(null!, null!, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
        }

        [Fact]
        public void ThrowsOnNullCallback()
        {
            // assert
            Assert.Throws<ArgumentNullException>("callback", () =>
            {
                // act
                new SafeTimer(NullLogger<SafeTimer>.Instance, null!, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
        }
    }
}