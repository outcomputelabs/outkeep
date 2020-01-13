using Microsoft.Extensions.Hosting;
using Outkeep.Core;
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

        [Fact]
        public void UseOutkeepServerWithoutContextCallsActionsOnBuild()
        {
            var builder = new HostBuilder();
            var called = false;

            var result = builder.UseOutkeepServer(outkeep =>
            {
                called = true;
            });

            Assert.False(called);

            builder.Build();

            Assert.Same(builder, result);
            Assert.True(called);
        }

        [Fact]
        public void UseOutkeepReusesBuilder()
        {
            // arrange
            var builder = new HostBuilder();

            // act
            IOutkeepServerBuilder? outkeep1 = null;
            builder.UseOutkeepServer((context, outkeep) =>
            {
                outkeep1 = outkeep;
            });

            IOutkeepServerBuilder? outkeep2 = null;
            builder.UseOutkeepServer((context, outkeep) =>
            {
                outkeep2 = outkeep;
            });

            builder.Build();

            // assert
            Assert.NotNull(outkeep1);
            Assert.NotNull(outkeep2);
            Assert.Same(outkeep1, outkeep2);
        }
    }
}