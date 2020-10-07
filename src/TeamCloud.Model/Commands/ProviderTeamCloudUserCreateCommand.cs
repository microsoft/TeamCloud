/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderTeamCloudUserCreateCommand : ProviderCreateCommand<User, ProviderTeamCloudUserCreateCommandResult>
    {
        public ProviderTeamCloudUserCreateCommand(User user, User payload, Guid? commandId = default) : base(user, payload, commandId)
        { }
    }
}
