/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderTeamCloudUserDeleteCommand : ProviderDeleteCommand<User, ProviderTeamCloudUserDeleteCommandResult>
    {
        public ProviderTeamCloudUserDeleteCommand(User user, User payload, Guid? commandId = null) : base(user, payload, commandId)
        { }
    }
}
