using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime;
using System;
using System.Reflection;

namespace Outkeep.Registry
{
    /// <summary>
    /// From a given instance of <see cref="RegistryStateAttribute"/> applied to a <see cref="IRegistryState{TState}"/> constructor parameter,
    /// finds the appropriate <see cref="IRegistryStateFactory"/> and uses it to create a configured <see cref="IRegistryState{TState}"/> instance.
    /// </summary>
    public class RegistryStateAttributeMapper : IAttributeToFactoryMapper<RegistryStateAttribute>
    {
        /// <summary>
        /// Caches the unbounded generic factory create method to avoid redundant alocations.
        /// </summary>
        private static readonly MethodInfo _create = typeof(IRegistryStateFactory).GetMethod(nameof(IRegistryStateFactory.Create));

        public Factory<IGrainActivationContext, object> GetFactory(ParameterInfo parameter, RegistryStateAttribute metadata)
        {
            if (parameter is null) throw new ArgumentNullException(nameof(parameter));
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));

            var types = parameter.ParameterType.GetGenericArguments();
            var genericCreate = _create.MakeGenericMethod(types);

            return context => Create(genericCreate, context, metadata);
        }

        private object Create(MethodInfo genericCreate, IGrainActivationContext context, IRegistryStateConfiguration config)
        {
            var factory = context.ActivationServices.GetRequiredService<IRegistryStateFactory>();

            var args = new object[] { context, config };
            return genericCreate.Invoke(factory, args);
        }
    }
}