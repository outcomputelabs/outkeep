using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Outkeep.Api.Http.Properties;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Api.Http
{
    public class RestApiHostedService : IHostedService
    {
        private readonly ILogger<RestApiHostedService> logger;
        private readonly IWebHost host;

        public RestApiHostedService(ILogger<RestApiHostedService> logger, IEnumerable<ILoggerProvider> loggerProviders, IGrainFactory factory)
        {
            this.logger = logger;

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
                    services.AddSingleton(factory);

                    services.AddMvc()
                        .SetCompatibilityVersion(CompatibilityVersion.Latest)
                        .AddApplicationPart(GetType().Assembly)
                        .AddControllersAsServices();

                    services.AddSwaggerGen(options =>
                    {
                        options.SwaggerDoc("v0", new Info
                        {
                            Title = Resources.OutkeepHttpApi,
                            Version = "v0"
                        });
                    });
                })
                .Configure(app =>
                {
                    app.UseCors(nameof(RestApiHostedService));
                    app.UseSwagger(options =>
                    {
                    });
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v0/swagger.json", nameof(Outkeep));
                    });
                    app.UseMiddleware<ActivityMiddleware>();
                    app.UseMvc();
                })
                .UseUrls("http://localhost:8081")
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