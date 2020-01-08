using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Grains.Tests
{
    [Collection(nameof(ClusterCollection))]
    public class CacheDirectorGrainServiceTests
    {
        private readonly ClusterFixture _fixture;

        public CacheDirectorGrainServiceTests(ClusterFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task Pings()
        {
            // arrange
            var client = _fixture.Cluster.Client.GetGrain<ICacheDirectorGrainServiceTestGrain>(0);

            // act
            await client.PingAsync().ConfigureAwait(false);

            // assert
            Assert.True(true);
        }
    }
}