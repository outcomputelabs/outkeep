﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Outkeep.Implementations;
using Serilog;
using System;
using System.Threading.Tasks;
using Outkeep.Hosting;

namespace Outkeep.Application.Standalone
{
    internal class Program
    {
        private static Task Main(string[] args)
        {
            return new HostBuilder()
                .ConfigureHostConfiguration(config =>
                {
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    config
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables(nameof(Outkeep))
                        .AddUserSecrets<Program>()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddSerilog(
                        new LoggerConfiguration()
                            .WriteTo.Console()
                            .CreateLogger(),
                        true);
                })
                .UseOutkeepServer((context, outkeep) =>
                {
                    outkeep.Configure<DistributedCacheOptions>(options =>
                    {
                        options.ExpirationPolicyEvaluationPeriod = context.Configuration.GetValue<TimeSpan>("Outkeep:DistributedCache:ExpirationPolicyEvaluationPeriod");
                    });
                })
                .RunConsoleAsync();
        }
    }
}
