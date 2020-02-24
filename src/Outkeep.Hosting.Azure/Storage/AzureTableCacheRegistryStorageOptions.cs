using System.ComponentModel.DataAnnotations;

namespace Outkeep.Hosting.Azure.Storage
{
    public class AzureTableCacheRegistryStorageOptions
    {
        [Required]
        public string? ConnectionString { get; set; }

        [Required]
        public string? TableName { get; set; }

        public bool UseRestExecutorForCosmosEndpoint { get; set; }
    }
}