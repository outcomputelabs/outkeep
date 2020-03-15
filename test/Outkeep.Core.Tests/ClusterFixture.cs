using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.TestingHost;
using Orleans.Timers;
using Outkeep.Caching;
using Outkeep.Caching.Memory;
using Outkeep.Core.Tests.Fakes;
using Outkeep.Governance;
using Outkeep.HealthChecks;
using Outkeep.Time;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core.Tests
{
    public sealed class ClusterFixture : IDisposable
    {
        private readonly string _clusterId = TestClusterBuilder.CreateClusterId();
        private static readonly ConcurrentDictionary<string, IServiceProvider> _primarySiloServiceProviders = new ConcurrentDictionary<string, IServiceProvider>();

        public TestCluster Cluster { get; }
        public IServiceProvider PrimarySiloServiceProvider => _primarySiloServiceProviders[_clusterId];

        public ClusterFixture()
        {
            var builder = new TestClusterBuilder(1)
                .AddSiloBuilderConfigurator<SiloBuilderConfigurator>();

            builder.Options.ClusterId = _clusterId;

            Cluster = builder.Build();
            Cluster.Deploy();
        }

        public void Dispose()
        {
            Cluster.StopAllSilos();
            Cluster.Dispose();
        }

        private class SiloBuilderConfigurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder
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

                        // add the memory cache registry
                        services.AddMemoryCacheRegistry();

                        // add other services
                        services
                            .AddSingleton<ISystemClock, SystemClock>()
                            .AddSafeTimer();
                    })
                    .AddMemoryGrainStorage(OutkeepProviderNames.OutkeepCache)
                    .AddStartupTask((provider, token) =>
                    {
                        // capture the service provider of the primary silo for use in tests
                        var options = provider.GetRequiredService<IOptions<ClusterOptions>>().Value;
                        _primarySiloServiceProviders.TryAdd(options.ClusterId, provider);
                        return Task.CompletedTask;
                    })
                    .AddStartupTask(async (provider, token) =>
                    {
                        // seed the memory cache registry with test data
                        var grain = provider.GetRequiredService<IGrainFactory>().GetMemoryCacheRegistryGrain();
                        await grain.WriteEntityAsync(new RegistryEntity("A", 1, DateTimeOffset.UtcNow.AddHours(1), TimeSpan.FromHours(1), null)).ConfigureAwait(true);
                        await grain.WriteEntityAsync(new RegistryEntity("B", 2, DateTimeOffset.UtcNow.AddHours(2), TimeSpan.FromHours(2), null)).ConfigureAwait(true);
                        await grain.WriteEntityAsync(new RegistryEntity("C", 3, DateTimeOffset.UtcNow.AddHours(3), TimeSpan.FromHours(3), null)).ConfigureAwait(true);
                    });
            }
        }
    }
}