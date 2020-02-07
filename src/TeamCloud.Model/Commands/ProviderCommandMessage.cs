/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;

namespace TeamCloud.Model.Commands
{
    public class ProviderCommandMessage
    {
        public ICommand Command { get; set; }

        public string CallbackUrl { get; set; }

        public ProviderCommandMessage() { }

        public ProviderCommandMessage(ICommand command, string callbackUrl)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            CallbackUrl = callbackUrl ?? throw new ArgumentNullException(nameof(callbackUrl));
        }

        [JsonIgnore]
        public Guid? CommandId => Command?.CommandId;

        [JsonIgnore]
        public Type CommandType => Command?.GetType();

    }
}
