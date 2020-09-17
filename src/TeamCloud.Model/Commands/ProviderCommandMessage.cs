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

        public new IProviderCommand Command
        {
            get => base.Command as IProviderCommand;
            set => base.Command = value;
        }

        public string CallbackUrl { get; set; }
    }
}
