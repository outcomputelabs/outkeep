using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outkeep.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Api.Rest
{
    public class RestApiHostedService : IHostedService
    {
        private readonly ILogger<RestApiHostedService> logger;
        private readonly IWebHost host;

        public RestApiHostedService(ILogger<RestApiHostedService> logger, IEnumerable<ILoggerProvider> loggerProviders, IOutkeepClient client, IOptions<RestApiServerOptions> apiOptions)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (loggerProviders == null) throw new ArgumentNullException(nameof(loggerProviders));
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (apiOptions == null) throw new ArgumentNullException(nameof(apiOptions));

            host = WebHost
                .CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    foreach (var provider in loggerProviders)
                    {
                        logging.AddProvider(provider);
                    }
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(client);

                    services.AddMvc()
                        .SetCompatibilityVersion(CompatibilityVersion.Latest)
                        .AddApplicationPart(GetType().Assembly)
                        .AddControllersAsServices();
                })
                .Configure(app =>
                {
                    app.UseCors(nameof(RestApiHostedService));
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v0/swagger.json", nameof(Outkeep));
                    });
                    app.UseMiddleware<ActivityMiddleware>();
                    app.UseMvc();
                })
                .UseUrls(apiOptions.Value.Uri.AbsoluteUri)
                .Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogOutkeepRestApiStarting();

            await host.StartAsync(cancellationToken).ConfigureAwait(false);

            logger.LogOutkeepRestApiStarted();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogOutkeepRestApiStopping();

            await host.StopAsync(cancellationToken).ConfigureAwait(false);

            logger.LogOutkeepRestApiStopped();
        }
    }
}