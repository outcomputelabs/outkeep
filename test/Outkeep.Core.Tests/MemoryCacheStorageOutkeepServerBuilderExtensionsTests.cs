using Microsoft.Extensions.DependencyInjection;
using Outkeep.Core.Storage;
using Outkeep.Core.Tests.Fakes;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class MemoryCacheStorageOutkeepServerBuilderExtensionsTests
    {
        [Fact]
        public void AddMemoryCacheStorageThrowsOnNullBuilder()
        {
            // arrange
            IOutkeepServerBuilder builder = null!;

            // act
            void action() => builder.AddMemoryCacheStorage();

            // act and assert
            Assert.Throws<ArgumentNullException>(nameof(builder), action);
        }

        [Fact]
        public void AddMemoryCacheStorageAddsServices()
        {
            // arrange
            var builder = new FakeOutkeepServerBuilder();

            // act
            var result = builder.AddMemoryCacheStorage();
            var service = builder.BuildServiceProvider(null!, null!).GetService<ICacheStorage>();

            // assert
            Assert.Same(builder, result);
            Assert.IsType<MemoryCacheStorage>(service);
        }
    }
}