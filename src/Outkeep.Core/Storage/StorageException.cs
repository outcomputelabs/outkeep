using System;
using System.Runtime.Serialization;

namespace Outkeep.Core.Storage
{
    /// <summary>
    /// The general exception class for outkeep related storage exceptions.
    /// Outkeep extensions can derive from this class or an appropriate base class for their own storage exceptions.
    /// </summary>
    [Serializable]
    public class StorageException : OutkeepException
    {
        public StorageException()
        {
        }

        public StorageException(string message) : base(message)
        {
        }

        public StorageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StorageException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}