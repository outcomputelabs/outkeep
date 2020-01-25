using Outkeep.Core;
using System;
using System.Runtime.Serialization;

namespace Outkeep.Grains.Governance
{
    [Serializable]
    public class ResourceGovernorException : OutkeepException
    {
        protected ResourceGovernorException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public ResourceGovernorException()
        {
        }

        public ResourceGovernorException(string message) : base(message)
        {
        }

        public ResourceGovernorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}