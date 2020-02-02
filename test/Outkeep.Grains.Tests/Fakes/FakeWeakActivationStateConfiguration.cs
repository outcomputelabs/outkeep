using Outkeep.Governance;

namespace Outkeep.Grains.Tests.Fakes
{
    public class FakeWeakActivationStateConfiguration : IWeakActivationStateConfiguration
    {
        public string? ResourceGovernorName { get; set; }
    }
}