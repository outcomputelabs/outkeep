using System.ComponentModel.DataAnnotations;

namespace Outkeep.Hosting.Standalone.Caching
{
    public class SqliteCacheRegistryStorageOptions
    {
        public const string DefaultMigrationsHistoryTable = "MigrationsHistory";

        [Required]
        public string ConnectionString { get; set; } = null!;

        [Required]
        public string MigrationsHistoryTable { get; set; } = DefaultMigrationsHistoryTable;

        public string? MigrationsHistoryTableSchema { get; set; }
    }
}