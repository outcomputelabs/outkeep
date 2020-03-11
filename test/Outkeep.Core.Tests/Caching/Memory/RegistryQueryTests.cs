using Moq;
using Orleans;
using Outkeep.Caching;
using Outkeep.Caching.Memory;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests.Caching.Memory
{
    public class RegistryQueryTests
    {
        [Fact]
        public async Task Test()
        {
            var factory = Mock.Of<IGrainFactory>();
            var provider = new RegistryQueryProvider(factory);
            var query = new RegistryQuery<RegistryEntry>(provider);

            var key = "SomeKey";

            var result = await query.Where(x => x.Key == key).ToImmutableListAsync().ConfigureAwait(false);

            Assert.NotNull(result);
        }
    }
}