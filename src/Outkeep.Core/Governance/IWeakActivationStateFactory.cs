using Orleans.Runtime;

namespace Outkeep.Governance
{
    internal interface IWeakActivationStateFactory
    {
        IWeakActivationState<TState> Create<TState>(IGrainActivationContext context, IWeakActivationStateConfiguration config) where TState : IWeakActivationFactor, new();
    }
}