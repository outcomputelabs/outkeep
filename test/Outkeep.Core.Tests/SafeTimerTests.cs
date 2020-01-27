using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class SafeTimerTests
    {
        [Fact]
        public void ThrowsOnNullCallback()
        {
            // arrange
            Func<object?, Task>? callback = null;

            // act
            Assert.Throws<ArgumentNullException>(nameof(callback), () =>
            {
                // act
                new SafeTimer(callback!, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
        }
    }
}