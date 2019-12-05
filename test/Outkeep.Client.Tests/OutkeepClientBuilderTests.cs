using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Xunit;

namespace Outkeep.Client.Tests
{
    public class OutkeepClientBuilderTests
    {
        private class SomeOptions
        {
            public string SomeProperty { get; set; }
        }

        [Fact]
        public void BuildsServices()
        {
            // arrange
            var services = new ServiceCollection();
            var context = new HostBuilderContext(new Dictionary<object, object> { { "SomeProperty", "SomeValue" } });
            var builder = new OutkeepClientBuilder();

            // act
            builder.ConfigureServices((context, services) =>
            {
                services.Configure<SomeOptions>(options =>
                {
                    options.SomeProperty = context.Properties["SomeProperty"].ToString();
                });
            });

            // assert no services added yet
            Assert.Empty(services);

            // act
            builder.Build(context, services);

            // assert hosted service added
            Assert.Contains(services, x => x.ServiceType == typeof(IHostedService) && x.ImplementationType == typeof(OutkeepClientHostedService) && x.Lifetime == ServiceLifetime.Singleton);

            // assert options service added
            Assert.Contains(services, x => x.ServiceType == typeof(IOptions<>));

            // assert options value set
            Assert.Equal("SomeValue", services.BuildServiceProvider().GetRequiredService<IOptions<SomeOptions>>().Value.SomeProperty);
        }
    }
}