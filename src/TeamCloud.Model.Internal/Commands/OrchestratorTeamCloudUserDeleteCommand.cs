/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorTeamCloudUserDeleteCommand : OrchestratorCommand<UserDocument, OrchestratorTeamCloudUserDeleteCommandResult>
    {
        public OrchestratorTeamCloudUserDeleteCommand(UserDocument user, UserDocument payload) : base(user, payload) { }
    }
}
