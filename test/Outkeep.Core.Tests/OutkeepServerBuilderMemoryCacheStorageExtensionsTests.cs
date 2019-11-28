using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class OutkeepServerBuilderMemoryCacheStorageExtensionsTests
    {
        [Fact]
        public void AddMemoryCacheStorageConfiguresServices()
        {
            var services = new ServiceCollection();

            var builder = Mock.Of<IOutkeepServerBuilder>();
            Mock.Get(builder)
                .Setup(x => x.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
                .Callback((Action<HostBuilderContext, IServiceCollection> action) => action(null, services))
                .Returns(builder);

            var result = builder.AddMemoryCacheStorage();

            Mock.Get(builder).VerifyAll();
            Assert.Same(builder, result);

            var provider = services.BuildServiceProvider();
            Assert.IsType<MemoryCacheStorage>(services.BuildServiceProvider().GetService<ICacheStorage>());
        }
    }
}