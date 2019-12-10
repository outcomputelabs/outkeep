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
        public async Task WithDefaultOnTimeoutReturnsCompletedTask()
        {
            // arrange
            Task<int> task = Task.FromResult(1);

            // act
            var result = await task.WithDefaultOnTimeout(0, TimeSpan.Zero).ConfigureAwait(false);

            // assert
            Assert.Equal(1, result);
        }
    }
}