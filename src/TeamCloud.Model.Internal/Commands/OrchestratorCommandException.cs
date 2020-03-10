using System;
using System.Runtime.Serialization;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Commands
{
    [Serializable]
    public class OrchestratorCommandException : CommandException
    {
        public OrchestratorCommandException()
        { }

        public OrchestratorCommandException(string message) : base(message)
        { }

        public OrchestratorCommandException(string message, Exception inner) : base(message, inner)
        { }

        public OrchestratorCommandException(string message, IOrchestratorCommand orchestratorCommand) : base(message, orchestratorCommand)
        { }

        public OrchestratorCommandException(string message, IOrchestratorCommand orchestratorCommand, Exception inner) : base(message, orchestratorCommand, inner)
        { }

        protected OrchestratorCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
