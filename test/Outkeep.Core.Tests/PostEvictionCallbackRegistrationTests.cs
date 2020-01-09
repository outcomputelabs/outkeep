using Moq;
using Outkeep.Core.Caching;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class PostEvictionCallbackRegistrationTests
    {
        [Fact]
        public async Task Cycles()
        {
            // arrange
            var count = 0;
            void callback(object? s) { count++; }
            var state = new object();
            var scheduler = TaskScheduler.Default;
            var context = Mock.Of<ICacheEntryContext>();

            // act
            var registration = new PostEvictionCallbackRegistration(callback, state, scheduler, context);

            // assert
            Assert.Equal(0, count);

            // act
            await registration.InvokeAsync().ConfigureAwait(false);

            // assert
            Assert.Equal(1, count);

            // act
            await registration.InvokeAsync().ConfigureAwait(false);

            // assert
            Assert.Equal(2, count);

            // act
            registration.Dispose();
            Mock.Get(context).Verify(x => x.OnPostEvictionCallbackRegistrationDisposed(registration));
        }
    }
}