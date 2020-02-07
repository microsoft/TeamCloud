/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Model.Commands
{
    public class ProviderCommandMessage : CommandMessage
    {
        public string CallbackUrl { get; set; }

        public ProviderCommandMessage() { }

        public ProviderCommandMessage(ICommand command, string callbackUrl)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            CallbackUrl = callbackUrl ?? throw new ArgumentNullException(nameof(callbackUrl));
        }
    }
}
