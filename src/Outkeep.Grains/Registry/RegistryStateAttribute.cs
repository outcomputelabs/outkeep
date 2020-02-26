using Orleans;
using System;

namespace Outkeep.Registry
{
    /// <summary>
    /// Configures an <see cref="IRegistryState{TState}"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RegistryStateAttribute : Attribute, IFacetMetadata, IRegistryStateConfiguration
    {
        public string? StorageName { get; set; }

        public string? ContainerName { get; set; }

        public string? RegistryName { get; set; }
    }
}