using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Outkeep.Core.Storage;
using Outkeep.Core.Tests.Fakes;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class NullGrainStorageOutkeepServerBuilderExtensionsTests
    {
        [Fact]
        public void AddNullGrainStorageAddsServices()
        {
            // arrange
            var builder = new FakeOutkeepServerBuilder();
            var name = Guid.NewGuid().ToString();

            // act
            var result = builder.AddNullGrainStorage(name);

            // assert
            Assert.Same(builder, result);
            Assert.IsType<NullGrainStorage>(builder.BuildServiceProvider(null!, null!).GetServiceByName<IGrainStorage>(name));
        }

        [Fact]
        public void AddNullGrainStorageAsDefaultAddsServices()
        {
            // arrange
            var builder = new FakeOutkeepServerBuilder();

            // act
            var result = builder.AddNullGrainStorageAsDefault();

            // assert
            Assert.Same(builder, result);
            Assert.IsType<NullGrainStorage>(builder.BuildServiceProvider(null!, null!).GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
        }
    }
}