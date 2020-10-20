/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorTeamCloudUserUpdateCommand : OrchestratorUpdateCommand<UserDocument, OrchestratorTeamCloudUserUpdateCommandResult>
    {
        public OrchestratorTeamCloudUserUpdateCommand(UserDocument user, UserDocument payload) : base(user, payload) { }
    }
}
