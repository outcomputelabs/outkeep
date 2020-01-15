using Microsoft.Extensions.DependencyInjection;
using Outkeep.Core.Storage;
using Outkeep.Core.Tests.Fakes;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class NullCacheStorageOutkeepServerBuilderExtensionsTests
    {
        [Fact]
        public void AddNullCacheStorageThrowsOnNullBuilder()
        {
            // arrange
            IOutkeepServerBuilder builder = null!;

            // act
            void action() => builder.AddNullCacheStorage();

            // act and assert
            Assert.Throws<ArgumentNullException>(nameof(builder), action);
        }

        [Fact]
        public void AddNullCacheStorageAddsServices()
        {
            // arrange
            var builder = new FakeOutkeepServerBuilder();

            // act
            var result = builder.AddNullCacheStorage();
            var service = builder.BuildServiceProvider(null!, null!).GetService<ICacheStorage>();

            // assert
            Assert.Same(builder, result);
            Assert.IsType<NullCacheStorage>(service);
        }
    }
}