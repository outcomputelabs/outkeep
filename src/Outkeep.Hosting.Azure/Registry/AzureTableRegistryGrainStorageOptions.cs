using System.ComponentModel.DataAnnotations;

namespace Outkeep.Hosting.Azure.Registry
{
    public class AzureTableRegistryGrainStorageOptions
    {
        [Required]
        public string? ConnectionString { get; set; }

        [Required]
        public string? TableName { get; set; }

        /// <summary>
        /// The partition name to use if this registry is a global registry.
        /// </summary>
        [Required]
        public string GlobalRegistryPartitionKey { get; set; } = "GlobalRegistry";

        /// <summary>
        /// If <see cref="true"/>, this registry will be shared across all grains that use it.
        /// If <see cref="false"/>, each grain instance will have its own partition withing the registry.
        /// Defaults to <see cref="false"/>.
        /// </summary>
        public bool IsGlobalRegistry { get; set; } = false;

        public bool UseRestExecutorForCosmosEndpoint { get; set; } = false;
    }
}