using Microsoft.Extensions.Hosting;
using Xunit;

namespace Outkeep.Client.Tests
{
    public class HostBuilderExtensionsTests
    {
        [Fact]
        public void UseOutkeepClientCallsActionsOnBuild1()
        {
            var builder = new HostBuilder();
            var called = false;

            var result = builder.UseOutkeepClient((context, outkeep) =>
            {
                called = true;
            });

            Assert.False(called);

            builder.Build();

            Assert.Same(builder, result);
            Assert.True(called);
        }

        [Fact]
        public void UseOutkeepClientCallsActionsOnBuild2()
        {
            var builder = new HostBuilder();
            var called = false;

            var result = builder.UseOutkeepClient(outkeep =>
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