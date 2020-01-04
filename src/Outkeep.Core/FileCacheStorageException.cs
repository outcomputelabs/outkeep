using System;
using System.Runtime.Serialization;

namespace Outkeep.Core
{
    [Serializable]
    public class FileCacheStorageException : StorageException
    {
        public FileCacheStorageException()
            : this(null, null, null, null, null)
        {
        }

        public FileCacheStorageException(string? message)
            : this(message, null, null, null, null)
        {
        }

        public FileCacheStorageException(string? message, string? path, string? key)
            : this(message, path, key, null, null)
        {
        }

        public FileCacheStorageException(string? message, string? path, string? key, string? otherKey)
            : this(message, path, key, otherKey, null)
        {
        }

        public FileCacheStorageException(string? message, Exception? innerException)
            : this(message, null, null, null, innerException)
        {
        }

        public FileCacheStorageException(string? message, string? path, string? key, Exception? innerException)
            : this(message, path, key, null, innerException)
        {
        }

        public FileCacheStorageException(string? message, string? path, string? key, string? otherKey, Exception? innerException) : base(message!, innerException!)
        {
            Path = path;
            Key = key;
            OtherKey = otherKey;
        }

        public string? Path { get; }
        public string? Key { get; }
        public string? OtherKey { get; }

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