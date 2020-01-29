using Microsoft.Extensions.DependencyInjection;
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

            var named = !string.IsNullOrWhiteSpace(config.ResourceGovernorName);

            var governor = named
                ? context.ActivationServices.GetServiceByName<IResourceGovernor>(config.ResourceGovernorName)
                : context.ActivationServices.GetService<IResourceGovernor>();

            if (governor is null)
            {
                throw named
                    ? new BadWeakActivationConfigException(Resources.Exception_NoResourceGovernorNamed_X_FoundForGrainType_X.Format(config.ResourceGovernorName!, context.GrainType.FullName))
                    : new BadWeakActivationConfigException(Resources.Exception_NoDefaultResourceGovernorFoundForGrainType_X.Format(context.GrainType.FullName));
            }

            return new WeakActivationState<TState>(context, governor);
        }
    }
}