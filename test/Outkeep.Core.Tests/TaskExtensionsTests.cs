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
            Task<int> task = null;

            // act and assert
            await Assert.ThrowsAsync<ArgumentNullException>(nameof(task), () => task.WithDefaultOnTimeout(0, TimeSpan.Zero)).ConfigureAwait(false);
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
    }
}