/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderTeamCloudUserDeleteCommand : ProviderCommand<User, ProviderTeamCloudUserDeleteCommandResult>
    {
        public ProviderTeamCloudUserDeleteCommand(Uri api, User user, User payload, Guid? commandId = null) : base(api, user, payload, commandId)
        { }
    }
}
