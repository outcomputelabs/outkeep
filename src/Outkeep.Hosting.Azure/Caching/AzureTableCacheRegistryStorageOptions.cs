using System.ComponentModel.DataAnnotations;

namespace Outkeep.Caching
{
    public class AzureTableCacheRegistryStorageOptions
    {
        public const string DefaultConnectionString = "UseDevelopmentStorage=true";
        public const string DefaultDataRowKey = "Data";
        public const string DefaultTableName = "OutkeepCacheRegistry";

        [Required]
        public string ConnectionString { get; set; } = DefaultConnectionString;

        [Required]
        public string TableName { get; set; } = DefaultTableName;

        [Required]
        public string DataRowKey { get; set; } = DefaultDataRowKey;
    }
}