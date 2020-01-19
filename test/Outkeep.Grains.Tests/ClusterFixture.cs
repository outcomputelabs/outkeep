using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.TestingHost;
using System;
using System.Collections.Concurrent;

namespace Outkeep.Grains.Tests
{
    public sealed class ClusterFixture : IDisposable
    {
        private readonly string _clusterId = TestClusterBuilder.CreateClusterId();

        public TestCluster Cluster { get; }

        public ClusterFixture()
        {
            var builder = new TestClusterBuilder(1)
                .AddSiloBuilderConfigurator<SiloBuilderConfigurator>()
                .AddClientBuilderConfigurator<ClientBuilderConfigurator>();

            builder.Options.ClusterId = _clusterId;

            Cluster = builder.Build();
            Cluster.Deploy();
        }

        public void Dispose()
        {
            Cluster.StopAllSilos();
            Cluster.Dispose();
        }

        private static readonly ConcurrentDictionary<string, IServiceProvider> _siloServiceProvider =
            new ConcurrentDictionary<string, IServiceProvider>();

        private class SiloBuilderConfigurator : ISiloBuilderConfigurator
        {
            public void Configure(ISiloHostBuilder hostBuilder)
            {
                hostBuilder
                    .AddMemoryGrainStorage(OutkeepProviderNames.OutkeepCache)
                    .ConfigureApplicationParts(apm =>
                    {
                        apm.AddApplicationPart(typeof(EchoGrain).Assembly).WithReferences();
                        apm.AddApplicationPart(typeof(ClusterFixture).Assembly);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services
                            .AddCacheDirector(options =>
                            {
                                options.Capacity = 1000;
                            })
                            .AddSystemClock()
                            .AddCacheGrainContext()
                            .Configure<CacheGrainOptions>(options =>
                            {
                                options.ReactivePollingTimeout = TimeSpan.FromSeconds(5);
                            });
                    })
                    .UseServiceProviderFactory(services =>
                    {
                        var provider = services.BuildServiceProvider();
                        var clusterId = provider.GetRequiredService<IOptions<ClusterOptions>>().Value.ClusterId;

                        _siloServiceProvider.TryAdd(clusterId, provider);

                        return provider;
                    });
            }
        }

        private class ClientBuilderConfigurator : IClientBuilderConfigurator
        {
            public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
            {
                // placeholder
            }
        }

        public IServiceProvider PrimarySiloServiceProvider
        {
            get
            {
                if (_siloServiceProvider.TryGetValue(_clusterId, out var provider))
                {
                    return provider;
                }
                throw new InvalidOperationException();
            }
        }
    }
}