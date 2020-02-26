using Orleans.Runtime;
using System;

namespace Outkeep.Registry
{
    /// <summary>
    /// Creates a new instance of <see cref="RegistryState{TState}"/> given the specified input.
    /// This factory is used with <see cref="RegistryStateAttribute"/> and <see cref="RegistryStateAttributeMapper"/> to take parameter-based user configuration into account.
    /// </summary>
    internal class RegistryStateFactory : IRegistryStateFactory
    {
        public IRegistryState<TState> Create<TState>(IGrainActivationContext context, IRegistryStateConfiguration config) where TState : class, new()
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (config is null) throw new ArgumentNullException(nameof(config));

            var state = new RegistryState<TState>(context, config);
            state.Participate(context.ObservableLifecycle);
            return state;
        }
    }
}