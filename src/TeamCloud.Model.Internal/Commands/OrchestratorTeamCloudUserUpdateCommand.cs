/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorTeamCloudUserUpdateCommand : OrchestratorCommand<UserDocument, OrchestratorTeamCloudUserUpdateCommandResult>
    {
        public OrchestratorTeamCloudUserUpdateCommand(UserDocument user, UserDocument payload) : base(user, payload) { }
    }
}
