using Microsoft.Extensions.Logging.Abstractions;
using Outkeep.Core.Tests.Fakes;
using Outkeep.Properties;
using Outkeep.Timers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
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

        [Fact]
        public async Task TicksOnce()
        {
            // arrange
            var count = 0;

            // act
            using (var timer = new SafeTimer(NullLogger<SafeTimer>.Instance, _ => { count += 1; return Task.CompletedTask; }, null, TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan))
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }

            // assert
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task TicksMany()
        {
            // arrange
            var count = 0;

            // act
            using (var timer = new SafeTimer(NullLogger<SafeTimer>.Instance, _ => { count += 1; return Task.CompletedTask; }, null, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100)))
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }

            // assert
            Assert.Equal(10, count);
        }

        [Fact]
        public async Task TicksNone()
        {
            // arrange
            var count = 0;

            // act
            using (var timer = new SafeTimer(NullLogger<SafeTimer>.Instance, _ => { count += 1; return Task.CompletedTask; }, null, Timeout.InfiniteTimeSpan, TimeSpan.FromMilliseconds(100)))
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }

            // assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task TicksSome()
        {
            // arrange
            var count = 0;

            // act
            using (var timer = new SafeTimer(NullLogger<SafeTimer>.Instance, _ => { count += 1; return Task.Delay(150); }, null, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100)))
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }

            // assert
            Assert.Equal(5, count);
        }

        [Fact]
        public async Task TicksOnError()
        {
            // arrange
            var count = 0;

            // act
            using (var timer = new SafeTimer(NullLogger<SafeTimer>.Instance, _ => { count += 1; throw new InvalidOperationException(); }, null, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100)))
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }

            // assert
            Assert.Equal(10, count);
        }

        [Fact]
        [SuppressMessage("Minor Code Smell", "S3626:Jump statements should not be redundant")]
        public async Task LogsOnImmediateCancellation()
        {
            // arrange
            var exception = new OperationCanceledException();
            var logger = new FakeLogger<SafeTimer>();

            // act
            using (var timer = new SafeTimer(logger, _ => throw exception, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }

            // assert
            Assert.Single(logger.Output, Resources.Log_TimerTickWasCancelled);
        }

        [Fact]
        public async Task LogsOnDelayedCancellation()
        {
            // arrange
            var logger = new FakeLogger<SafeTimer>();

            // act
            using (var timer = new SafeTimer(logger, _ => Task.FromCanceled(new CancellationToken(true)), null, TimeSpan.Zero, Timeout.InfiniteTimeSpan))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }

            // assert
            Assert.Single(logger.Output, Resources.Log_TimerTickWasCancelled);
        }

        [Fact]
        [SuppressMessage("Minor Code Smell", "S3626:Jump statements should not be redundant")]
        public async Task LogsOnImmediateFault()
        {
            // arrange
            var exception = new InvalidOperationException();
            var logger = new FakeLogger<SafeTimer>();

            // act
            using (var timer = new SafeTimer(logger, _ => throw exception, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }

            // assert
            Assert.Single(logger.Output, Resources.Log_TimerTickHasFaulted);
        }

        [Fact]
        public async Task LogsOnDelayedFault()
        {
            // arrange
            var exception = new InvalidOperationException();
            var logger = new FakeLogger<SafeTimer>();

            // act
            using (var timer = new SafeTimer(logger, _ => Task.FromException(exception), null, TimeSpan.Zero, Timeout.InfiniteTimeSpan))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }

            // assert
            Assert.Single(logger.Output, Resources.Log_TimerTickHasFaulted);
        }
    }
}