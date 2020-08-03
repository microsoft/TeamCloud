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
        public ProviderTeamCloudUserDeleteCommand(Uri baseApi, User user, User payload, Guid? commandId = null) : base(baseApi, user, payload, commandId)
        { }
    }
}
