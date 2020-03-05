using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.TestingHost;
using Orleans.Timers;
using Outkeep.Caching;
using Outkeep.Core.Tests.Fakes;
using Outkeep.Governance;
using Outkeep.HealthChecks;
using Outkeep.Time;
using System;
using System.Collections.Concurrent;

namespace Outkeep.Core.Tests
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
                    .ConfigureApplicationParts(apm =>
                    {
                        apm.AddApplicationPart(typeof(EchoGrain).Assembly).WithReferences();
                        apm.AddApplicationPart(typeof(ClusterFixture).Assembly);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // override the reactive polling timeout to make timed tests faster
                        services.Configure<CacheOptions>(options =>
                        {
                            options.ReactivePollingTimeout = TimeSpan.FromSeconds(5);
                        });

                        // add weak activation facet
                        services.AddSingleton<IWeakActivationStateFactory, WeakActivationStateFactory>();
                        services.AddSingleton<IAttributeToFactoryMapper<WeakActivationStateAttribute>, WeakActivationStateAttributeMapper>();

                        // add memory resource governor
                        services.AddMemoryResourceGovernor(OutkeepProviderNames.OutkeepMemoryResourceGovernor);

                        // add test resource governor
                        services.AddSingletonNamedService<IResourceGovernor, FakeResourceGovernor>("WeakActivationTestGovernor");

                        // add test timer registry
                        services
                            .AddSingleton<FakeTimerRegistry>()
                            .AddSingleton<ITimerRegistry>(sp => sp.GetService<FakeTimerRegistry>());

                        // add other services
                        services
                            .AddSingleton<ISystemClock, SystemClock>()
                            .AddSafeTimer();
                    })
                    .AddMemoryGrainStorage(OutkeepProviderNames.OutkeepCache)
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