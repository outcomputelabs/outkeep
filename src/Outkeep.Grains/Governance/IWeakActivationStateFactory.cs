using Orleans.Runtime;

namespace Outkeep.Grains.Governance
{
    public interface IWeakActivationStateFactory
    {
        IWeakActivationState<TState> Create<TState>(IGrainActivationContext context, IWeakActivationStateConfiguration config) where TState : IWeakActivationFactor, new();
    }
}