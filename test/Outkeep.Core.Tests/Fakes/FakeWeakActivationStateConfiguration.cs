using Outkeep.Governance;

namespace Outkeep.Core.Tests.Fakes
{
    public class FakeWeakActivationStateConfiguration : IWeakActivationStateConfiguration
    {
        public string? ResourceGovernorName { get; set; }
    }
}