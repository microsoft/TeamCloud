/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderCommandMessage
    {
        public ICommand Command { get; set; }

        public Provider Provider { get; set; }

        public string CallbackUrl { get; set; }

        public ProviderCommandMessage() { }

        public ProviderCommandMessage(ICommand command, Provider provider, string callbackUrl)
        {
            Command = command;
            Provider = provider;
            CallbackUrl = callbackUrl;
        }

        [JsonIgnore]
        public Guid CommandId => Command.CommandId;

        public ProviderCommandResultMessage CreateResult()
            => new ProviderCommandResultMessage(Command.CreateResult(), Provider);
    }

    public class ProviderCommandResultMessage
    {
        public ICommandResult CommandResult { get; set; }

        public Provider Provider { get; set; }

        public ProviderCommandResultMessage() { }

        public ProviderCommandResultMessage(ICommandResult commandResult, Provider provider)
        {
            CommandResult = commandResult;
            Provider = provider;
        }

        [JsonIgnore]
        public Guid CommandId => CommandResult.CommandId;

        [JsonIgnore]
        public List<Exception> Exceptions => CommandResult.Exceptions;
    }
}
