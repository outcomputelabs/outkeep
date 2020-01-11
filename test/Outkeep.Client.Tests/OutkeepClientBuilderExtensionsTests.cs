using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using Xunit;

namespace Outkeep.Client.Tests
{
    public class OutkeepClientBuilderExtensionsTests
    {
        [Fact]
        public void ConfigureServicesWithoutContext()
        {
            // arrange
            var registered = false;
            var builder = Mock.Of<IOutkeepClientBuilder>();
            Mock.Get(builder)
                .Setup(x => x.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
                .Callback((Action<HostBuilderContext, IServiceCollection> action) => action(null!, null!))
                .Returns(builder);

            // act
            var result = OutkeepClientBuilderExtensions.ConfigureServices(builder, services => { registered = true; });

            // assert
            Assert.Same(builder, result);
            Assert.True(registered);
            Mock.Get(builder).VerifyAll();
        }

        [Fact]
        public void ConfigureServicesThrowsOnNullBuilder()
        {
            // arrange
            IOutkeepClientBuilder builder = null!;

            // act
            void action() { builder.ConfigureServices(services => { }); }

            // assert
            Assert.Throws<ArgumentNullException>(nameof(builder), action);
        }
    }
}