using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Outkeep.Core;
using System;
using Xunit;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Outkeep.Hosting.Tests
{
    public class OutkeepServerBuilderExtensionsTests
    {
        private class MyOptions
        {
            public int SomeProperty { get; set; }
        }

        [Fact]
        public void ConfiguresOptions()
        {
            var services = new ServiceCollection();

            var builder = Mock.Of<IOutkeepServerBuilder>();
            Mock.Get(builder)
                .Setup(x => x.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
                .Callback((Action<HostBuilderContext, IServiceCollection> action) => action(null!, services))
                .Returns(builder);

            builder.Configure<MyOptions>(options =>
            {
                options.SomeProperty = 123;
            });

            var options = services.BuildServiceProvider().GetService<IOptions<MyOptions>>();
            Assert.NotNull(options);
            Assert.Equal(123, options.Value.SomeProperty);

            Mock.Get(builder).VerifyNoOtherCalls();
        }

        [Fact]
        public void ConfigureServicesWithServiceCollectionOnly()
        {
            // arrange
            var builder = Mock.Of<IOutkeepServerBuilder>();
            var services = Mock.Of<IServiceCollection>();
            Action<HostBuilderContext, IServiceCollection>? action = null;
            Mock.Get(builder)
                .Setup(x => x.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
                .Callback((Action<HostBuilderContext, IServiceCollection> a) => { action = a; })
                .Returns(builder);

            // act
            IServiceCollection? input = null;
            var result = builder.ConfigureServices(services =>
            {
                input = services;
            });

            // assert
            Assert.Same(builder, result);
            Assert.NotNull(action);
            action?.Invoke(null!, services);
            Assert.NotNull(input);
            Assert.Same(services, input);
        }

        [Fact]
        public void ConfigureSiloWithSiloOnly()
        {
            // arrange
            var builder = Mock.Of<IOutkeepServerBuilder>();
            var silo = Mock.Of<ISiloBuilder>();
            Action<HostBuilderContext, ISiloBuilder>? action = null;
            Mock.Get(builder)
                .Setup(x => x.ConfigureSilo(It.IsAny<Action<HostBuilderContext, ISiloBuilder>>()))
                .Callback((Action<HostBuilderContext, ISiloBuilder> a) => { action = a; })
                .Returns(builder);

            // act
            ISiloBuilder? input = null;
            var result = builder.ConfigureSilo(silo =>
            {
                input = silo;
            });

            // assert
            Assert.Same(builder, result);
            Assert.NotNull(action);
            action?.Invoke(null!, silo);
            Assert.NotNull(input);
            Assert.Same(silo, input);
        }
    }
}