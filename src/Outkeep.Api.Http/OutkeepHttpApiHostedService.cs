using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Outkeep.Api.Http.Properties;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Api.Http
{
    internal class OutkeepHttpApiHostedService : IHostedService
    {
        private readonly ILogger<OutkeepHttpApiHostedService> _logger;
        private readonly IHost _host;
        private readonly OutkeepHttpApiServerOptions _options;

        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "N/A")]
        public OutkeepHttpApiHostedService(
            ILogger<OutkeepHttpApiHostedService> logger,
            IOptions<OutkeepHttpApiServerOptions> apiOptions,
            IEnumerable<ILoggerProvider> loggerProviders,
            IGrainFactory factory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = apiOptions?.Value ?? throw new ArgumentNullException(nameof(apiOptions));

            _host = Host
                .CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    foreach (var provider in loggerProviders)
                    {
                        logging.AddProvider(provider);
                    }
                })
                .ConfigureWebHostDefaults(web =>
                {
                    web.ConfigureKestrel(options =>
                    {
                        if (_options.MaxRequestBodySize.HasValue)
                        {
                            options.Limits.MaxRequestBodySize = _options.MaxRequestBodySize;
                        }
                    });

                    web.ConfigureServices(services =>
                    {
                        services.AddControllers();

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

                        services.AddApiVersioning(options =>
                        {
                            options.ReportApiVersions = true;
                            options.DefaultApiVersion = new ApiVersion(1, 0);
                            options.AssumeDefaultVersionWhenUnspecified = true;
                        });

                        services.AddVersionedApiExplorer(options =>
                        {
                            options.GroupNameFormat = "'v'VVV";
                        });

                        services.AddSwaggerGen()
                            .AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenOptionsConfigurator>();

                        services.AddSingleton(factory);

                        services.AddActivityMiddleware();
                    });

                    web.Configure((context, app) =>
                    {
                        app.UseHttpsRedirection();
                        app.UseRouting();
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.UseSwagger();
                        app.UseSwaggerUI(options =>
                        {
                            foreach (var description in app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion))
                            {
                                var vx = description.GroupName.ToLowerInvariant();
                                options.SwaggerEndpoint($"/swagger/{vx}/swagger.json", vx);
                            }
                        });

                        app.UseActivityMiddleware();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });

                    if (_options.ApiUri != null)
                    {
                        web.UseUrls(_options.ApiUri.ToString());
                    }
                })
                .Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.OutkeepHttpApiStarting(_logger);

            await _host.StartAsync(cancellationToken).ConfigureAwait(false);

            Log.OutkeepHttpApiStarted(_logger);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.OutkeepHttpApiStopping(_logger);

            await _host.StopAsync(cancellationToken).ConfigureAwait(false);

            Log.OutkeepHttpApiStopped(_logger);
        }

        private static class Log
        {
            private static readonly Action<ILogger, Exception?> OutkeepHttpApiStartingAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepHttpApiStarting)),
                    Resources.OutkeepHttpApiStarting);

            public static void OutkeepHttpApiStarting(ILogger logger) =>
                OutkeepHttpApiStartingAction(logger, null);

            private static readonly Action<ILogger, Exception?> OutkeepHttpApiStartedAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepHttpApiStarted)),
                    Resources.OutkeepHttpApiStarted);

            public static void OutkeepHttpApiStarted(ILogger logger) =>
                OutkeepHttpApiStartedAction(logger, null);

            private static readonly Action<ILogger, Exception?> OutkeepHttpApiStoppingAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepHttpApiStopping)),
                    Resources.OutkeepHttpApiStopping);

            public static void OutkeepHttpApiStopping(ILogger logger) =>
                OutkeepHttpApiStoppingAction(logger, null);

            private static readonly Action<ILogger, Exception?> OutkeepHttpApiStoppedAction =
                LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepHttpApiStopped)),
                    Resources.OutkeepHttpApiStopped);

            public static void OutkeepHttpApiStopped(ILogger logger) =>
                OutkeepHttpApiStoppedAction(logger, null);
        }
    }
}