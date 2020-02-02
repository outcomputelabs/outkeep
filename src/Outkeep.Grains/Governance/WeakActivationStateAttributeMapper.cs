using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime;
using System;
using System.Reflection;

namespace Outkeep.Governance
{
    public class WeakActivationStateAttributeMapper : IAttributeToFactoryMapper<WeakActivationStateAttribute>
    {
        private static readonly MethodInfo _create = typeof(IWeakActivationStateFactory).GetMethod(nameof(IWeakActivationStateFactory.Create));

        public Factory<IGrainActivationContext, object> GetFactory(ParameterInfo parameter, WeakActivationStateAttribute metadata)
        {
            if (parameter is null) throw new ArgumentNullException(nameof(parameter));
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));

            var types = parameter.ParameterType.GetGenericArguments();
            var genericCreate = _create.MakeGenericMethod(types);

            return context => Create(context, genericCreate, metadata);
        }

        private object Create(IGrainActivationContext context, MethodInfo genericCreate, IWeakActivationStateConfiguration config)
        {
            var factory = context.ActivationServices.GetRequiredService<IWeakActivationStateFactory>();

            var args = new object[] { context, config };
            return genericCreate.Invoke(factory, args);
        }
    }
}