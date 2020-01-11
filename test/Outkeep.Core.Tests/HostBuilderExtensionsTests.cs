using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outkeep.Core.Caching;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class HostBuilderExtensionsTests
    {
        [Fact]
        public void UseCacheDirector()
        {
            // arrange
            var builder = new HostBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ISystemClock>(NullClock.Default);
            });

            // act
            builder.UseCacheDirector((context, options) =>
            {
                options.Capacity = 10000;
            });

            // assert
            var services = builder.Build().Services;
            var director = services.GetRequiredService<ICacheDirector>();
            Assert.NotNull(director);
            Assert.Equal(10000, director.Capacity);
        }
    }
}