using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Outkeep.Core.Tests.Fakes;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class OutkeepServerBuilderExtensionsTests
    {
        [Fact]
        public void ConfigureConfiguresServices()
        {
            // arrange
            var builder = new FakeOutkeepServerBuilder();
            var value = Guid.NewGuid();

            // act
            builder.Configure<FakeOptions>(options =>
            {
                options.Value = value;
            });

            // assert
            Assert.Equal(value, builder.BuildServiceProvider(null!, null!).GetService<IOptions<FakeOptions>>().Value.Value);
        }

        [Fact]
        public void ConfigureServicesConfiguresServices()
        {
            // arrange
            var builder = new FakeOutkeepServerBuilder();

            // act
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IFakeService, FakeService>();
            });

            // assert
            Assert.IsType<FakeService>(builder.BuildServiceProvider(null!, null!).GetService<IFakeService>());
        }
    }
}