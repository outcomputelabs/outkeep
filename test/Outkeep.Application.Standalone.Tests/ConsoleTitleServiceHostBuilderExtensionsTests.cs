using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Outkeep.Application.Standalone.Tests
{
    public class ConsoleTitleServiceHostBuilderExtensionsTests
    {
        [Fact]
        public void UseConsoleTitleAddsServices()
        {
            // arrange
            var builder = new HostBuilder();

            // act
            var result = builder.UseConsoleTitle();

            // assert
            Assert.Same(result, builder);
            Assert.IsType<ConsoleTitleService>(builder.Build().Services.GetRequiredService<IHostedService>());
        }
    }
}