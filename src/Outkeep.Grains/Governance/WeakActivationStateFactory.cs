using Orleans.Runtime;
using Outkeep.Properties;
using System;

namespace Outkeep.Governance
{
    public class WeakActivationStateFactory : IWeakActivationStateFactory
    {
        public IWeakActivationState<TState> Create<TState>(IGrainActivationContext context, IWeakActivationStateConfiguration config)
            where TState : IWeakActivationFactor, new()
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (config is null) throw new ArgumentNullException(nameof(config));

            var name = config.ResourceGovernorName ?? OutkeepProviderNames.OutkeepDefault;
            var governor = context.ActivationServices.GetServiceByName<IResourceGovernor>(name);

            if (governor is null)
            {
                throw new BadWeakActivationConfigException(Resources.Exception_NoResourceGovernorNamed_X_FoundForGrainType_X.Format(name, context.GrainType.FullName));
            }

            var state = new WeakActivationState<TState>(context, governor);
            state.Participate(context.ObservableLifecycle);
            return state;
        }
    }
}