using Microsoft.Azure.Cosmos.Table;

namespace Outkeep.Hosting.Azure.Registry
{
    internal class AzureTableRegistryEntity : TableEntity
    {
        public byte[]? Payload { get; set; }
    }
}