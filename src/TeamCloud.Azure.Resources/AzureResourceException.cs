using System;
using System.Runtime.Serialization;

namespace TeamCloud.Azure.Resources
{

    [Serializable]
    public class AzureResourceException : Exception
    {
        public AzureResourceException() { }

        public AzureResourceException(string message) : base(message) { }

        public AzureResourceException(string message, Exception inner) : base(message, inner) { }

        protected AzureResourceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
