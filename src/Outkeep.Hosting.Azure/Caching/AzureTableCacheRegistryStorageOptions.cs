using System.ComponentModel.DataAnnotations;

namespace Outkeep.Caching
{
    public class AzureTableCacheRegistryStorageOptions
    {
        public const string DefaultConnectionString = "UseDevelopmentStorage=true";
        public const string DefaultMainPartitionKey = "Main";
        public const string DefaultTableName = "OutkeepCacheRegistry";

        [Required]
        public string ConnectionString { get; set; } = DefaultConnectionString;

        [Required]
        public string MainPartitionKey { get; set; } = DefaultMainPartitionKey;

        [Required]
        public string TableName { get; set; } = DefaultTableName;
    }
}