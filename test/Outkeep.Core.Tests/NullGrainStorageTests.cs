using Outkeep.Core.Storage;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class NullGrainStorageTests
    {
        [Fact]
        public async Task Lifecycle()
        {
            // arrange
            var storage = new NullGrainStorage();

            // act - all calls are noop
            await storage.WriteStateAsync(null!, null!, null!).ConfigureAwait(false);
            await storage.ReadStateAsync(null!, null!, null!).ConfigureAwait(false);
            await storage.ClearStateAsync(null!, null!, null!).ConfigureAwait(false);

            // assert
            Assert.True(true);
        }
    }
}