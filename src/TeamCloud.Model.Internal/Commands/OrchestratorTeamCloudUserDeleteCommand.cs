/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorTeamCloudUserDeleteCommand : OrchestratorDeleteCommand<UserDocument, OrchestratorTeamCloudUserDeleteCommandResult>
    {
        public OrchestratorTeamCloudUserDeleteCommand(UserDocument user, UserDocument payload) : base(user, payload) { }
    }
}
