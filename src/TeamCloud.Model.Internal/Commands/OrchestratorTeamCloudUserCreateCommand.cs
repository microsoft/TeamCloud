/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorTeamCloudUserCreateCommand : OrchestratorCommand<UserDocument, OrchestratorTeamCloudUserCreateCommandResult>
    {
        public OrchestratorTeamCloudUserCreateCommand(UserDocument user, UserDocument payload) : base(user, payload) { }
    }
}
