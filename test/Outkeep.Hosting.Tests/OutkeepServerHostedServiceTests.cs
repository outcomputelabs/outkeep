using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Hosting.Tests
{
    public class OutkeepServerHostedServiceTests
    {
        [Fact]
        public async Task Cycles()
        {
            // arrange
            var logger = Mock.Of<ILogger<OutkeepServerHostedService>>();
            var service = new OutkeepServerHostedService(logger);

            // act
            await service.StartAsync(default).ConfigureAwait(false);
            await service.StopAsync(default).ConfigureAwait(false);

            // assert
            Assert.True(true);
        }
    }
}