using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Outkeep.Api.Http;
using Outkeep.Application.Standalone.Properties;
using Outkeep.Dashboard;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Application.Standalone
{
    /// <summary>
    /// A simple startup task to show the active ports on the console title for developer convenience.
    /// </summary>
    internal class ConsoleTitleService : IHostedService
    {
        private readonly EndpointOptions _endpointOptions;
        private readonly OutkeepHttpApiServerOptions _httpApiOptions;
        private readonly OutkeepDashboardOptions _outkeepDashboardOptions;

        public ConsoleTitleService(IOptions<EndpointOptions> endpointOptions, IOptions<OutkeepHttpApiServerOptions> httpApiOptions, IOptions<OutkeepDashboardOptions> outkeepDashboardOptions)
        {
            _endpointOptions = endpointOptions?.Value ?? throw new ArgumentNullException(nameof(endpointOptions));
            _httpApiOptions = httpApiOptions?.Value ?? throw new ArgumentNullException(nameof(httpApiOptions));
            _outkeepDashboardOptions = outkeepDashboardOptions?.Value ?? throw new ArgumentNullException(nameof(outkeepDashboardOptions));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Environment.UserInteractive)
            {
                try
                {
                    Console.Title = Resources.Console_Title.Format(nameof(Standalone), _endpointOptions.SiloPort, _endpointOptions.GatewayPort, _httpApiOptions.ApiUri?.Port ?? -1, _outkeepDashboardOptions.Url.Port);
                }
                catch (PlatformNotSupportedException)
                {
                    // noop - some platforms do not support the console title
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}