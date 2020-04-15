using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<OutkeepDashboardService>();
                })
                .Build())
            {
                // act - sanity test only for now
                await host.StartAsync().ConfigureAwait(false);
                await host.StopAsync().ConfigureAwait(false);
            }

            // assert
            Assert.True(true);
        }
    }
}