using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
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
            var provider = Mock.Of<IServiceProvider>();
            var service = new OutkeepDashboardService(logger, provider);

            // act
            await service.StartAsync(default).ConfigureAwait(true);
            await service.StopAsync(default).ConfigureAwait(true);

            // assert
            Assert.True(true);
        }
    }
}