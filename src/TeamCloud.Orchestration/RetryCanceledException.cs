using System;
using System.Runtime.Serialization;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestration
{

    [Serializable]
    public class RetryCanceledException : Exception
    {
        public RetryCanceledException()
        { }

        public RetryCanceledException(string message) : base(message)
        { }

        public RetryCanceledException(string message, Exception inner) : base(message, inner?.AsSerializable())
        { }

        protected RetryCanceledException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
