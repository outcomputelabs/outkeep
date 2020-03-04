using System;
using System.Runtime.Serialization;

namespace Outkeep.Governance
{
    [Serializable]
    public class BadWeakActivationConfigException : OutkeepException
    {
        public BadWeakActivationConfigException()
        {
        }

        public BadWeakActivationConfigException(string message)
            : base(message)
        {
        }

        public BadWeakActivationConfigException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BadWeakActivationConfigException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}