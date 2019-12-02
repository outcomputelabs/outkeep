using System;
using System.Runtime.Serialization;

namespace Outkeep.Core
{
    [Serializable]
    public class FileCacheStorageException : StorageException
    {
        public FileCacheStorageException()
        {
        }

        public FileCacheStorageException(string message) : base(message)
        {
        }

        public FileCacheStorageException(string message, string path, string key) : base(message)
        {
            Path = path;
            Key = key;
        }

        public FileCacheStorageException(string message, string path, string key, string otherKey) : base(message)
        {
            Path = path;
            Key = key;
            OtherKey = otherKey;
        }

        public FileCacheStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public FileCacheStorageException(string message, string path, string key, Exception innerException) : base(message, innerException)
        {
            Path = path;
            Key = key;
        }

        public FileCacheStorageException(string message, string path, string key, string otherKey, Exception innerException) : base(message, innerException)
        {
            Path = path;
            Key = key;
            OtherKey = otherKey;
        }

        public string Path { get; }
        public string Key { get; }
        public string OtherKey { get; }

        public override string ToString() => $"{nameof(FileCacheStorageException)}: Key='{Key}', OtherKey='{OtherKey}', Path='{Path}', Message='{Message}', InnerException='{InnerException}'";

        protected FileCacheStorageException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            Path = serializationInfo.GetString(nameof(Path));
            Key = serializationInfo.GetString(nameof(Key));
            OtherKey = serializationInfo.GetString(nameof(OtherKey));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(Path), Path);
            info.AddValue(nameof(Key), Key);
            info.AddValue(nameof(OtherKey), OtherKey);

            base.GetObjectData(info, context);
        }
    }
}