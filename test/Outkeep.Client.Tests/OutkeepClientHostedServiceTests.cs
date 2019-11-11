using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Client.Tests
{
    public class OutkeepClientHostedServiceTests
    {
        [Fact]
        public async Task Cycles()
        {
            var logger = Mock.Of<ILogger<OutkeepClientHostedService>>();
            var service = new OutkeepClientHostedService(logger);

            await service.StartAsync(default).ConfigureAwait(false);
            await service.StopAsync(default).ConfigureAwait(false);
        }
    }
}