using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class NullTimerEntryTests
    {
        [Fact]
        public void SetsDisposedOnDisposed()
        {
            // arrange
            var entry = new NullTimerEntry(_ => Task.CompletedTask, null, TimeSpan.Zero, TimeSpan.Zero);

            // assert
            Assert.False(entry.IsDisposed);

            // act
            entry.Dispose();

            // assert
            Assert.True(entry.IsDisposed);
        }
    }
}