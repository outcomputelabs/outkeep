using Moq;
using Orleans;
using Orleans.Runtime;
using Outkeep.Governance;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class GrainControlExtensionTests
    {
        [Fact]
        public async Task DeactivateOnIdleAsyncCallsDeactivateOnRuntime()
        {
            // arrange
            var grainInstance = new Mock<Grain>().Object;
            var context = Mock.Of<IGrainActivationContext>(x => x.GrainInstance == grainInstance);
            var runtime = Mock.Of<IGrainRuntime>();
            var extension = new GrainControlExtension(context, runtime);

            // act
            await extension.DeactivateOnIdleAsync().ConfigureAwait(false);

            // assert
            Mock.Get(runtime).Verify(x => x.DeactivateOnIdle(grainInstance));
        }
    }
}