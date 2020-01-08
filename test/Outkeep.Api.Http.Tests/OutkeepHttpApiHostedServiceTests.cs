using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class OutkeepHttpApiHostedServiceTests
    {
        [Fact]
        public async Task Cycles()
        {
            // arrange
            var logger = new NullLogger<OutkeepHttpApiHostedService>();
            var options = new OutkeepHttpApiServerOptions
            {
                ApiUri = new Uri("http://localhost:51234")
            };
            var loggerProviders = new ILoggerProvider[] { NullLoggerProvider.Instance };
            var grainFactory = Mock.Of<IGrainFactory>();

            // act
            var service = new OutkeepHttpApiHostedService(logger, Options.Create(options), loggerProviders, grainFactory);
            await service.StartAsync(default).ConfigureAwait(false);
            await service.StopAsync(default).ConfigureAwait(false);

            // assert
            Assert.True(true);
        }
    }
}