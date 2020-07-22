/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderTeamCloudUserCreateCommand : ProviderCommand<User, ProviderTeamCloudUserCreateCommandResult>
    {
        public ProviderTeamCloudUserCreateCommand(Uri api, User user, User payload, Guid? commandId = default) : base(api, user, payload, commandId)
        { }
    }
}
