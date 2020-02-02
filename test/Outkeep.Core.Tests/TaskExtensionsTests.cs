using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class TaskExtensionsTests
    {
        [Fact]
        public async Task WithDefaultOnTimeoutThrowsOnNullTask()
        {
            // arrange
            Task<int>? task = null;

            // act and assert
            await Assert.ThrowsAsync<ArgumentNullException>(nameof(task), () => task!.WithDefaultOnTimeout(0, TimeSpan.Zero)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WithDefaultOnTimeoutThrowsNegativeTimeout()
        {
            // arrange
            var task = Task.FromResult(1);
            var timeout = TimeSpan.FromSeconds(-1);

            // act and assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(nameof(timeout), () => task!.WithDefaultOnTimeout(0, timeout)).ConfigureAwait(false);
        }

        [Fact]
        public void WithDefaultOnTimeoutReturnsCompletedTask()
        {
            // arrange
            Task<int> task = Task.FromResult(1);

            // act
            var result = task.WithDefaultOnTimeout(0, TimeSpan.Zero);

            // assert
            Assert.Same(task, result);
        }

        [Fact]
        public void WithDefaultOnTimeoutReturnsTaskOnMaxTimeout()
        {
            // arrange
            var completion = new TaskCompletionSource<int>();

            // act
            var result = completion.Task.WithDefaultOnTimeout(0, TimeSpan.MaxValue);

            // assert
            Assert.Same(completion.Task, result);
        }

        [Fact]
        public void WithDefaultOnTimeoutReturnsDefaultOnZeroTimeout()
        {
            // arrange
            var completion = new TaskCompletionSource<int>();

            // act
            var result = completion.Task.WithDefaultOnTimeout(0, TimeSpan.Zero);

            // assert
            Assert.True(result.IsCompletedSuccessfully);
            Assert.Equal(0, result.Result);
        }

        [Fact]
        public void WithDefaultOnTimeoutCompletesWithinTimeout()
        {
            // arrange
            var completion = new TaskCompletionSource<int>();

            // act
            var result = completion.Task.WithDefaultOnTimeout(0, TimeSpan.FromMilliseconds(100));
            completion.SetResult(1);

            // assert
            result.Wait();
            Assert.Equal(1, result.Result);
        }

        [Fact]
        public void WithDefaultOnTimeoutReturnsDefaultOnTimeout()
        {
            // arrange
            var completion = new TaskCompletionSource<int>();

            // act
            var result = completion.Task.WithDefaultOnTimeout(0, TimeSpan.FromMilliseconds(100));

            // assert
            result.Wait();
            Assert.Equal(0, result.Result);
        }
    }
}