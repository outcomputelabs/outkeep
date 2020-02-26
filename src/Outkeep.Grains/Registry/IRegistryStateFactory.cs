using Orleans.Runtime;

namespace Outkeep.Registry
{
    internal interface IRegistryStateFactory
    {
        public IRegistryState<TState> Create<TState>(IGrainActivationContext context, IRegistryStateConfiguration config) where TState : class, new();
    }
}