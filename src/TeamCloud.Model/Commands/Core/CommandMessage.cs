/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;

namespace TeamCloud.Model.Commands.Core
{
    public interface ICommandMessage : IValidatable
    {
        ICommand Command { get; }

        [JsonIgnore]
        Guid? CommandId { get; }

        [JsonIgnore]
        Type CommandType { get; }
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
