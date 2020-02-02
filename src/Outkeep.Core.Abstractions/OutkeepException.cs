using System;
using System.Runtime.Serialization;

namespace Outkeep
{
    /// <summary>
    /// The general exception class for outkeep related exceptions.
    /// Outkeep extensions can derive from this class or an appropriate base class for their own exceptions.
    /// </summary>
    [Serializable]
    public class OutkeepException : Exception
    {
        public OutkeepException()
        {
        }

        public OutkeepException(string message) : base(message)
        {
        }

        public OutkeepException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected OutkeepException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}