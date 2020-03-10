using System;
using System.Runtime.Serialization;

namespace TeamCloud.Model.Commands.Core
{
    [Serializable]
    public class CommandException : Exception
    {
        public CommandException()
        { }

        public CommandException(string message) : base(message)
        { }

        public CommandException(string message, Exception inner) : base(message, inner)
        { }

        public CommandException(string message, ICommand command) : base(message)
            => Command = command;

        public CommandException(string message, ICommand command, Exception inner) : base(message, inner)
            => Command = command;

        public ICommand Command { get; set; }

        protected CommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Command = info.GetValue(nameof(Command), typeof(ICommand)) as ICommand;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Command), Command, typeof(ICommand));
        }
    }
}
