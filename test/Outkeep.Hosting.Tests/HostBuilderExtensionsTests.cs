using Microsoft.Extensions.Hosting;
using Xunit;

namespace Outkeep.Hosting.Tests
{
    public class HostBuilderExtensionsTests
    {
        [Fact]
        public void UseOutkeepServerCallsActionsOnBuild()
        {
            var builder = new HostBuilder();
            var called = false;

            var result = builder.UseOutkeepServer((context, outkeep) =>
            {
                called = true;
            });

            Assert.False(called);

            builder.Build();

            Assert.Same(builder, result);
            Assert.True(called);
        }
    }
}