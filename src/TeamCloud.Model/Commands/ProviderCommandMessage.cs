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
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            CallbackUrl = callbackUrl ?? throw new ArgumentNullException(nameof(callbackUrl));
        }

        [JsonIgnore]
        public Guid? CommandId => Command?.CommandId;

        public ProviderCommandResultMessage CreateResultMessage(ICommandResult commandResult = null)
        {
            commandResult = commandResult ?? Command.CreateResult();

            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            if (commandResult.CommandId != CommandId)
                throw new ArgumentException($"Result does not belong to command {this.CommandId}.", nameof(commandResult));

            return new ProviderCommandResultMessage(commandResult, Provider);
        }
    }

    public class ProviderCommandResultMessage
    {
        public ICommandResult CommandResult { get; set; }

        public Provider Provider { get; set; }

        public ProviderCommandResultMessage() { }

        public ProviderCommandResultMessage(ICommandResult commandResult, Provider provider)
        {
            CommandResult = commandResult ?? throw new ArgumentNullException(nameof(commandResult));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        [JsonIgnore]
        public Guid? CommandId => CommandResult?.CommandId;

        [JsonIgnore]
        public List<Exception> Exceptions => CommandResult?.Exceptions ?? new List<Exception>();
    }
}
