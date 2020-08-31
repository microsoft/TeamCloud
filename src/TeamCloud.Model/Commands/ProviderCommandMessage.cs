/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Commands
{
    public class ProviderCommandMessage : CommandMessage
    {
        public ProviderCommandMessage() : base()
        { }

        public ProviderCommandMessage(IProviderCommand command, string callbackUrl = null) : base(command)
        {
            CallbackUrl = callbackUrl;
        }

        public string CallbackUrl { get; set; }
    }
}
