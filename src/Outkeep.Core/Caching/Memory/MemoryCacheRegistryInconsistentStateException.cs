using System;
using System.Runtime.Serialization;

namespace Outkeep.Caching.Memory
{
    [Serializable]
    internal class MemoryCacheRegistryInconsistentStateException : OutkeepException
    {
        public MemoryCacheRegistryInconsistentStateException()
        {
        }

        public MemoryCacheRegistryInconsistentStateException(string message) : base(message)
        {
        }

        public MemoryCacheRegistryInconsistentStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MemoryCacheRegistryInconsistentStateException(string message, string? storedETag, string? currentETag)
            : base(message)
        {
            StoredEtag = storedETag;
            CurrentETag = currentETag;
        }

        public MemoryCacheRegistryInconsistentStateException(string message, string? storedETag, string? currentETag, Exception innerException)
            : base(message, innerException)
        {
            StoredEtag = storedETag;
            CurrentETag = currentETag;
        }

        public string? StoredEtag { get; }
        public string? CurrentETag { get; }

        protected MemoryCacheRegistryInconsistentStateException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            StoredEtag = serializationInfo.GetString(nameof(StoredEtag));
            CurrentETag = serializationInfo.GetString(nameof(CurrentETag));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null) throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(StoredEtag), StoredEtag);
            info.AddValue(nameof(CurrentETag), CurrentETag);

            base.GetObjectData(info, context);
        }
    }
}