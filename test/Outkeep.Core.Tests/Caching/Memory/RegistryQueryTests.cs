using Microsoft.Extensions.DependencyInjection;
using Outkeep.Caching;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests.Caching.Memory
{
    [Collection(nameof(ClusterCollection))]
    public class RegistryQueryTests
    {
        private readonly ClusterFixture _fixture;

        public RegistryQueryTests(ClusterFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task QueryingEverythingReturnsEverything()
        {
            var registry = _fixture.PrimarySiloServiceProvider.GetService<ICacheRegistry>();

            var result = await registry.CreateQuery().ToImmutableListAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Collection(result.OrderBy(x => x.Key),
                x => Assert.Equal("A", x.Key),
                x => Assert.Equal("B", x.Key),
                x => Assert.Equal("C", x.Key));
        }
    }
}