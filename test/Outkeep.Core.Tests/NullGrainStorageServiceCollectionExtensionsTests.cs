using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Outkeep.Core.Storage;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class NullGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddNullGrainStorage()
        {
            // arrange
            var services = new ServiceCollection();
            services.TryAddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));
            var name = Guid.NewGuid().ToString();

            // act
            var result = services.AddNullGrainStorage(name);

            // assert
            Assert.Same(services, result);
            Assert.IsType<NullGrainStorage>(services.BuildServiceProvider().GetRequiredServiceByName<IGrainStorage>(name));
        }

        [Fact]
        public void AddNullGrainStorageAsDefault()
        {
            // arrange
            var services = new ServiceCollection();
            services.TryAddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));

            // act
            var result = services.AddNullGrainStorageAsDefault();

            // assert
            Assert.Same(services, result);
            Assert.IsType<NullGrainStorage>(services.BuildServiceProvider().GetRequiredServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
        }
    }
}