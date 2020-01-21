using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Outkeep.Api.Http;
using Outkeep.Application.Standalone.Properties;
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
        private readonly OutkeepHttpApiServerOptions _httpApiOptions;

        public ConsoleTitleService(IOptions<OutkeepHttpApiServerOptions> httpApiOptions)
        {
            _httpApiOptions = httpApiOptions?.Value ?? throw new ArgumentNullException(nameof(httpApiOptions));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Environment.UserInteractive)
            {
                Console.Title = Resources.Console_Title.Format(_httpApiOptions.ApiUri?.Port ?? -1);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}