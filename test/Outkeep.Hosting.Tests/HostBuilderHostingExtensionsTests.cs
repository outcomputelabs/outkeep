using Microsoft.Extensions.Hosting;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Outkeep.Hosting.Tests
{
    public class HostBuilderHostingExtensionsTests
    {
        [Fact]
        public void UseOutkeepServerWithContext()
        {
            // arrange
            var properties = new Dictionary<object, object>();
            var builder = Mock.Of<IHostBuilder>(x => x.Properties == properties);

            // act
            builder.UseOutkeepServer((context, outkeep) =>
            {
            });

            // assert
            Assert.True(properties.TryGetValue(nameof(OutkeepServerBuilder), out var added));
            Assert.IsType<OutkeepServerBuilder>(added);
        }

        [Fact]
        public void UseOutkeepServerWithoutContext()
        {
            // arrange
            var properties = new Dictionary<object, object>();
            var builder = Mock.Of<IHostBuilder>(x => x.Properties == properties);

            // act
            builder.UseOutkeepServer(outkeep =>
            {
            });

            // assert
            Assert.True(properties.TryGetValue(nameof(OutkeepServerBuilder), out var added));
            Assert.IsType<OutkeepServerBuilder>(added);
        }
    }
}