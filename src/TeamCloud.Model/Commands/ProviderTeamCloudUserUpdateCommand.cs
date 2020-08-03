/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderTeamCloudUserUpdateCommand : ProviderCommand<User, ProviderTeamCloudUserUpdateCommandResult>
    {
        public ProviderTeamCloudUserUpdateCommand(Uri baseApi, User user, User payload, Guid? commandId = null) : base(baseApi, user, payload, commandId)
        { }
    }
}
