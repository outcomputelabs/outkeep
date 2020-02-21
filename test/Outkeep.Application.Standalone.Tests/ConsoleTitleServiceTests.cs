﻿using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Outkeep.Api.Http;
using Outkeep.Application.Standalone.Properties;
using Outkeep.Dashboard;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Application.Standalone.Tests
{
    public class ConsoleTitleServiceTests
    {
        [Fact]
        public async Task Lifecycle()
        {
            // arrange
            var endpointOptions = new EndpointOptions
            {
                SiloPort = 1234,
                GatewayPort = 2345
            };

            var httpApiOptions = new OutkeepHttpApiServerOptions
            {
                ApiUri = new Uri("http://localhost:3456")
            };

            var outkeepDashboardOptions = new OutkeepDashboardOptions
            {
                Url = new Uri("http://localhost:4567")
            };

            // act
            var service = new ConsoleTitleService(Options.Create(endpointOptions), Options.Create(httpApiOptions), Options.Create(outkeepDashboardOptions));
            await service.StartAsync(default).ConfigureAwait(false);

            // assert
            try
            {
                Assert.Equal(Console.Title, Resources.Console_Title.Format(nameof(Standalone), endpointOptions.SiloPort, endpointOptions.GatewayPort, httpApiOptions.ApiUri?.Port ?? -1, outkeepDashboardOptions.Url.Port));
            }
            catch (PlatformNotSupportedException)
            {
                // noop - some platforms do not support the console title
            }

            // act
            await service.StopAsync(default).ConfigureAwait(false);
        }
    }
}