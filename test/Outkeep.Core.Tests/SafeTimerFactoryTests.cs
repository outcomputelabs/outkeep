using Microsoft.Extensions.Logging.Abstractions;
using Outkeep.Timers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class SafeTimerFactoryTests
    {
        [Fact]
        public async Task TimerLifecycle()
        {
            // arrange
            var factory = new SafeTimerFactory(NullLogger<SafeTimer>.Instance);

            // act
            var effect = "";
            var timer = factory.Create(state => { effect += (string)state!; return Task.CompletedTask; }, "A", TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

            // assert
            await Task.Delay(50).ConfigureAwait(false);
            Assert.Equal("", effect);

            await Task.Delay(100).ConfigureAwait(false);
            Assert.Equal("A", effect);

            await Task.Delay(100).ConfigureAwait(false);
            Assert.Equal("AA", effect);

            timer.Dispose();
            await Task.Delay(100).ConfigureAwait(false);
            Assert.Equal("AA", effect);
        }
    }
}