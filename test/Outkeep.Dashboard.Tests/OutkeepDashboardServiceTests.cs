using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Dashboard.Tests
{
    public class OutkeepDashboardServiceTests
    {
        [Fact]
        public async Task Cycles()
        {
            // arrange
            var logger = NullLogger<OutkeepDashboardService>.Instance;
            var options = Options.Create(new OutkeepDashboardOptions());
            var service = new OutkeepDashboardService(logger, options);

            // act
            await service.StartAsync(default).ConfigureAwait(true);
            await service.StopAsync(default).ConfigureAwait(true);

            // assert
            Assert.True(true);
        }
    }
}