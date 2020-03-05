using Moq;
using Orleans;
using Orleans.Runtime;
using Outkeep.Governance;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class WeakActivationGrainExtensionTests
    {
        [Fact]
        public async Task DeactivateOnIdleAsyncCallsDeactivateOnRuntime()
        {
            // arrange
            var grainInstance = new Mock<Grain>().Object;
            var context = Mock.Of<IGrainActivationContext>(x => x.GrainInstance == grainInstance);
            var runtime = Mock.Of<IGrainRuntime>();
            var extension = new WeakActivationGrainExtension(context, runtime);

            // act
            await extension.DeactivateOnIdleAsync().ConfigureAwait(false);

            // assert
            Mock.Get(runtime).Verify(x => x.DeactivateOnIdle(grainInstance));
        }
    }
}