using Microsoft.Extensions.DependencyInjection;
using Outkeep.Core.Storage;
using Outkeep.Core.Tests.Fakes;
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
            var builder = new FakeOutkeepServerBuilder();

            // act
            var result = builder.AddMemoryCacheStorage();

            // assert
            Assert.Same(builder, result);

            var provider = builder.BuildServiceProvider(null!, null!);
            var service = provider.GetRequiredService<ICacheStorage>();
            Assert.IsType<MemoryCacheStorage>(service);
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

        [Fact]
        public void AddNullCacheStorageConfiguresServices()
        {
            // arrange
            var builder = new FakeOutkeepServerBuilder();

            // act
            var result = builder.AddNullCacheStorage();

            // assert
            Assert.Same(builder, result);

            var provider = builder.BuildServiceProvider(null!, null!);
            var service = provider.GetRequiredService<ICacheStorage>();
            Assert.IsType<NullCacheStorage>(service);
        }

        [Fact]
        public void AddNullCacheStorageThrowsOnNullBuilder()
        {
            // arrange
            IOutkeepServerBuilder? builder = null;

            // act
            void action() => builder!.AddNullCacheStorage();

            // assert
            Assert.Throws<ArgumentNullException>(nameof(builder), action);
        }
    }
}