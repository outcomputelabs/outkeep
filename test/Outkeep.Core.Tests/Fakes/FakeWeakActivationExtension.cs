using Outkeep.Governance;
using System.Threading.Tasks;

namespace Outkeep.Core.Tests.Fakes
{
    public class FakeWeakActivationExtension : IWeakActivationExtension
    {
        public Task DeactivateOnIdleAsync()
        {
            return Task.CompletedTask;
        }
    }
}