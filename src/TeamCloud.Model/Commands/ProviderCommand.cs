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
    public class ProviderCommand
    {
        public ICommand Command { get; set; }

        public Provider Provider { get; set; }

        public string CallbackUrl { get; set; }

        public ProviderCommand() { }

        public ProviderCommand(ICommand command, Provider provider, string callbackUrl)
        {
            Command = command;
            Provider = provider;
            CallbackUrl = callbackUrl;
        }

        [JsonIgnore]
        public Guid CommandId => Command.CommandId;
    }

    public class ProviderCommandResult : CommandResult<Dictionary<string, string>>
    {
        // public ICommandResult CommandResult { get; set; }

        public Provider Provider { get; set; }

        public Dictionary<string, string> Variables => this.Result;

        public string Error { get; set; }

        public ProviderCommandResult() { }

        public ProviderCommandResult(ProviderCommand providerCommand)
            : base(providerCommand.CommandId)
        {
            Provider = providerCommand.Provider;
        }

        public bool Succeeded => string.IsNullOrEmpty(Error);
    }
}
