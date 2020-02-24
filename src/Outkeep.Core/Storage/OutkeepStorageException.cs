using System;
using System.Runtime.Serialization;

namespace Outkeep.Storage
{
    /// <summary>
    /// The general exception class for outkeep related storage exceptions.
    /// Outkeep extensions can derive from this class or an appropriate base class for their own storage exceptions.
    /// </summary>
    [Serializable]
    public class OutkeepStorageException : OutkeepException
    {
        public OutkeepStorageException()
        {
        }

        public OutkeepStorageException(string message) : base(message)
        {
        }

        public OutkeepStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OutkeepStorageException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}