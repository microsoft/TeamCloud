/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Commands
{
    public class ProviderRegisterCommand : ProviderCommand<ProviderConfiguration, ProviderRegisterCommandResult>
    {
        public ProviderRegisterCommand(User user, ProviderConfiguration payload) : base(CommandAction.Register, user, payload)
        { }
    }
}
