using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Outkeep.Hosting.Azure.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOutkeepAzureClustering()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            var result = services.AddOutkeepAzureClustering();

            // assert
            Assert.Same(services, result);
        }
    }
}