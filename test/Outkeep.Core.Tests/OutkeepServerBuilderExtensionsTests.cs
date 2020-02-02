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
        public void ConfigureThrowsOnNullBuilder()
        {
            Assert.Throws<ArgumentNullException>("builder", () => OutkeepServerBuilderExtensions.Configure<FakeOptions>(null!, options => { }));
        }

        [Fact]
        public void ConfigureThrowsOnNullAction()
        {
            Assert.Throws<ArgumentNullException>("configure", () => OutkeepServerBuilderExtensions.Configure<FakeOptions>(new FakeOutkeepServerBuilder(), null!));
        }

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
        public void ConfigureServicesThrowsOnNullBuilder()
        {
            Assert.Throws<ArgumentNullException>("builder", () => OutkeepServerBuilderExtensions.ConfigureServices(null!, services => { }));
        }

        [Fact]
        public void ConfigureServicesThrowsOnNullAction()
        {
            Assert.Throws<ArgumentNullException>("configure", () => OutkeepServerBuilderExtensions.ConfigureServices(new FakeOutkeepServerBuilder(), null!));
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

        [Fact]
        public void ConfigureSiloThrowsOnNullBuilder()
        {
            Assert.Throws<ArgumentNullException>("builder", () => OutkeepServerBuilderExtensions.ConfigureSilo(null!, silo => { }));
        }

        [Fact]
        public void ConfigureSiloThrowsOnNullAction()
        {
            Assert.Throws<ArgumentNullException>("configure", () => OutkeepServerBuilderExtensions.ConfigureSilo(new FakeOutkeepServerBuilder(), null!));
        }
    }
}