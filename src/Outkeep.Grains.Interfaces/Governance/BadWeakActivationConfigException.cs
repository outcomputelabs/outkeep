using Outkeep.Core;
using System;
using System.Runtime.Serialization;

namespace Outkeep.Grains.Governance
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

        public BadWeakActivationConfigException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        protected BadWeakActivationConfigException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}