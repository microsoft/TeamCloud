/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderTeamCloudUserUpdateCommand : ProviderUpdateCommand<User, ProviderTeamCloudUserUpdateCommandResult>
    {
        public ProviderTeamCloudUserUpdateCommand(User user, User payload, Guid? commandId = null) : base(user, payload, commandId)
        { }
    }
}
