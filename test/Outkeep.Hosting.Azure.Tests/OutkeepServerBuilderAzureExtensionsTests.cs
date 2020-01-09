using Microsoft.Extensions.Hosting;
using Xunit;

namespace Outkeep.Hosting.Azure.Tests
{
    public class OutkeepServerBuilderAzureExtensionsTests
    {
        [Fact]
        public void UseAzure()
        {
            // arrange
            var builder = new HostBuilder();

            // act
            builder.UseOutkeepServer(outkeep =>
            {
                outkeep.UseAzure();
            });

            // assert
            Assert.True(true);
        }
    }
}