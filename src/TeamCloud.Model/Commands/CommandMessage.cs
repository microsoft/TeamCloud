using System;
using Newtonsoft.Json;

namespace TeamCloud.Model.Commands
{
    public interface ICommandMessage : IValidatable
    {
        public ICommand Command { get; }

        [JsonIgnore]
        public Guid? CommandId { get; }

        [JsonIgnore]
        public Type CommandType { get; }

    }

    public abstract class CommandMessage : ICommandMessage
    {
        protected CommandMessage()
        { }

        protected CommandMessage(ICommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public ICommand Command { get; set; }

        public Guid? CommandId => Command?.CommandId;

        public Type CommandType => Command?.GetType();
    }
}
