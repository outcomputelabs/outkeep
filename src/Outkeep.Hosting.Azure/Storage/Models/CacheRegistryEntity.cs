using Microsoft.Azure.Cosmos.Table;

namespace Outkeep.Hosting.Azure.Storage.Models
{
    public class CacheRegistryEntity : TableEntity
    {
        public string Key
        {
            get
            {
                return PartitionKey;
            }
            set
            {
                PartitionKey = value;
                RowKey = value;
            }
        }

        public int Size { get; set; }
    }
}