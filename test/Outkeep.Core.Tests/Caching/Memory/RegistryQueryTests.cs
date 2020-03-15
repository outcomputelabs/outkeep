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

        [Fact]
        public async Task QueryingWhereKeyEqualsReturnsFilteredResult()
        {
            var registry = _fixture.PrimarySiloServiceProvider.GetService<ICacheRegistry>();

            var result = await registry.CreateQuery().Where(x => x.Key == "B").ToImmutableListAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Collection(result, x => Assert.Equal("B", x.Key));
        }

        [Fact]
        public async Task QueryingWhereKeyLambdaEqualsReturnsFilteredResult()
        {
            var registry = _fixture.PrimarySiloServiceProvider.GetService<ICacheRegistry>();

            var key = "B";
            var result = await registry.CreateQuery().Where(x => x.Key == key).ToImmutableListAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Collection(result, x => Assert.Equal("B", x.Key));
        }
    }
}