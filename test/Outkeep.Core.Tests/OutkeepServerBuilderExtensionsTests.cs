using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Outkeep.Core.Storage;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class OutkeepServerBuilderExtensionsTests
    {
        [Fact]
        public void AddMemoryCacheStorageConfiguresServices()
        {
            // arrange
            var services = new ServiceCollection();

            var builder = Mock.Of<IOutkeepServerBuilder>();
            Mock.Get(builder)
                .Setup(x => x.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
                .Callback((Action<HostBuilderContext, IServiceCollection> action) => action(null!, services))
                .Returns(builder);

            // act
            var result = builder.AddMemoryCacheStorage();

            // assert
            Mock.Get(builder).VerifyAll();
            Assert.Same(builder, result);

            var provider = services.BuildServiceProvider();
            Assert.IsType<MemoryCacheStorage>(provider.GetService<ICacheStorage>());
        }

        [Fact]
        public void AddMemoryCacheStorageThrowsOnNullBuilder()
        {
            // arrange
            IOutkeepServerBuilder? builder = null;

            // act
            void action() => builder!.AddMemoryCacheStorage();

            // assert
            Assert.Throws<ArgumentNullException>(nameof(builder), action);
        }
    }
}