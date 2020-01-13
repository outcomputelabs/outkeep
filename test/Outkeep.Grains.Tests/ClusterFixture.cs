using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Concurrency;
using Orleans.Hosting;
using Orleans.TestingHost;
using Outkeep.Core;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains.Tests
{
    public sealed class ClusterFixture : IDisposable
    {
        public TestCluster Cluster { get; }

        public ClusterFixture()
        {
            Cluster = new TestClusterBuilder()
                .AddSiloBuilderConfigurator<SiloBuilderConfigurator>()
                .AddClientBuilderConfigurator<ClientBuilderConfigurator>()
                .Build();

            Cluster.Deploy();
        }

        public void Dispose()
        {
            Cluster.StopAllSilos();
            Cluster.Dispose();
        }

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
                        services
                            .AddCacheDirector(options =>
                            {
                                options.Capacity = 100;
                            })
                            .AddSystemClock();
                    })
                    .AddCacheDirectorGrainService();
            }
        }

        private class ClientBuilderConfigurator : IClientBuilderConfigurator
        {
            public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
            {
                // placeholder
            }
        }
    }

    [StatelessWorker]
    public class CacheDirectorGrainServiceTestGrain : Grain, ICacheDirectorGrainServiceTestGrain
    {
        private readonly ICacheDirectorGrainServiceClient _client;

        public CacheDirectorGrainServiceTestGrain(ICacheDirectorGrainServiceClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        [ReadOnly]
        public Task PingAsync() => _client.PingAsync();
    }

    public interface ICacheDirectorGrainServiceTestGrain : IGrainWithIntegerKey
    {
        Task PingAsync();
    }
}