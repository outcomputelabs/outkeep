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
    }
}