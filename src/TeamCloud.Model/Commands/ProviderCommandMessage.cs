/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Commands
{
    public class ProviderCommandMessage : CommandMessage
    {

        public ProviderCommandMessage() { }

        public ProviderCommandMessage(IProviderCommand command, string callbackUrl)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            CallbackUrl = callbackUrl ?? throw new ArgumentNullException(nameof(callbackUrl));
        }

        public string CallbackUrl { get; set; }
    }
}
