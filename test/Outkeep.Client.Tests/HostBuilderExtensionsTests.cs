using Microsoft.Extensions.Hosting;
using Xunit;

namespace Outkeep.Client.Tests
{
    public class HostBuilderExtensionsTests
    {
        [Fact]
        public void UseOutkeepClientCallsActions()
        {
            var builder = new HostBuilder();
            var called = false;

            var result = builder.UseOutkeepClient((context, outkeep) =>
            {
                called = true;
            });

            builder.Build();

            Assert.Same(builder, result);
            Assert.True(called);
        }
    }
}