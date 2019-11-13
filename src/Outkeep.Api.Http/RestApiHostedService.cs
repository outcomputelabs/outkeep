using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Api.Http
{
    internal class RestApiHostedService : IHostedService
    {
        private readonly ILogger<RestApiHostedService> logger;
        private readonly IWebHost host;

        public RestApiHostedService(
            ILogger<RestApiHostedService> logger,
            IOptions<RestApiServerOptions> apiOptions,
            IEnumerable<ILoggerProvider> loggerProviders,
            IGrainFactory factory)
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
                    // allow browser requests from anywhere for now
                    services.AddCors(options =>
                    {
                        options.AddDefaultPolicy(policy =>
                        {
                            policy
                                .AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        });
                    });

                    services.AddMvc()
                        .SetCompatibilityVersion(CompatibilityVersion.Latest)
                        .AddApplicationPart(GetType().Assembly)
                        .AddControllersAsServices();

                    services.AddApiVersioning(options =>
                    {
                        options.ReportApiVersions = true;
                        options.DefaultApiVersion = new ApiVersion(1, 0);
                    });

                    services.AddVersionedApiExplorer(options =>
                    {
                        options.GroupNameFormat = "'v'VVV";
                    });

                    services.AddTransient<IConfigureOptions<SwaggerGenOptions>, VersionedApiExplorerSwaggerOptionsConfigurator>();
                    services.AddSwaggerGen();

                    services.AddSingleton(factory);

                    // todo: make all these options configurable
                    if (apiOptions.Value.EnableCompression)
                    {
                        services.AddResponseCompression();
                        services.Configure<BrotliCompressionProviderOptions>(options =>
                        {
                            options.Level = CompressionLevel.Optimal;
                        });
                        services.Configure<GzipCompressionProviderOptions>(options =>
                        {
                            options.Level = CompressionLevel.Optimal;
                        });
                    }

                    services.AddActivityMiddleware();
                })
                .Configure(app =>
                {
                    if (apiOptions.Value.EnableCompression)
                    {
                        app.UseResponseCompression();
                    }

                    //app.UseRouting();
                    //app.UseAuthentication();
                    //app.UseAuthorization();
                    app.UseCors();
                    app.UseActivityMiddleware();

                    app.UseSwagger();

                    var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                    app.UseSwaggerUI(options =>
                    {
                        foreach (var description in provider.ApiVersionDescriptions)
                        {
                            options.SwaggerEndpoint(
                                $"/swagger/{description.GroupName}/swagger.json",
                                description.GroupName);
                        }
                    });
                })
                .UseUrls(apiOptions.Value.ApiUri.ToString())
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