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
            var provider = new RegistryQueryProvider();
            var query = new MemoryCacheRegistryQuery<ICacheRegistryEntryState>(provider);
            var key = "SomeKey";

            var result = await query.Where(x => x.Key == key).ToImmutableListAsync().ConfigureAwait(false);

            Assert.NotNull(result);
        }
    }
}