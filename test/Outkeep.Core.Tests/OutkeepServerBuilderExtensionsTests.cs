using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class OutkeepServerBuilderExtensionsTests
    {
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
    }
}