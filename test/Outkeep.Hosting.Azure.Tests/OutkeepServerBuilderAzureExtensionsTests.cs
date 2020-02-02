using Microsoft.Extensions.Hosting;
using Outkeep.Core;
using Outkeep.Hosting.Azure.Tests.Fakes;
using System.Collections.Generic;
using Xunit;

namespace Outkeep.Hosting.Azure.Tests
{
    public class OutkeepServerBuilderAzureExtensionsTests
    {
        [Fact]
        public void UseAzure()
        {
            // arrange
            var builder = new FakeOutkeepServerBuilder();
            var properties = new Dictionary<object, object>();
            var context = new HostBuilderContext(properties);
            var silo = new FakeSiloBuilder();

            // act
            builder.UseAzure();
            builder.BuildServiceProvider(context, silo);

            // assert
            Assert.True(true);
        }
    }
}