using System;
using System.Runtime.Serialization;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestration
{

    [Serializable]
    public class RetryCancelException : Exception
    {
        public RetryCancelException()
        { }

        public RetryCancelException(string message) : base(message)
        { }

        public RetryCancelException(string message, Exception inner) : base(message, inner?.AsSerializable())
        { }

        protected RetryCancelException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
