using System;
using System.Runtime.Serialization;

namespace Outkeep.Caching
{
    [Serializable]
    public class AzureTableCacheRegistryException : OutkeepException
    {
        public AzureTableCacheRegistryException()
        {
        }

        public AzureTableCacheRegistryException(string message) : base(message)
        {
        }

        public AzureTableCacheRegistryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AzureTableCacheRegistryException(string message, string tableName, string partitionKey, string rowKey) : base(message)
        {
            TableName = tableName;
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public AzureTableCacheRegistryException(string message, string tableName, string partitionKey, string rowKey, Exception innerException) : base(message, innerException)
        {
            TableName = tableName;
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        protected AzureTableCacheRegistryException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            TableName = serializationInfo.GetString(nameof(TableName));
            PartitionKey = serializationInfo.GetString(nameof(PartitionKey));
            RowKey = serializationInfo.GetString(nameof(RowKey));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null) throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(TableName), TableName);
            info.AddValue(nameof(PartitionKey), PartitionKey);
            info.AddValue(nameof(RowKey), RowKey);

            base.GetObjectData(info, context);
        }

        public string? TableName { get; }
        public string? PartitionKey { get; }
        public string? RowKey { get; }
    }
}