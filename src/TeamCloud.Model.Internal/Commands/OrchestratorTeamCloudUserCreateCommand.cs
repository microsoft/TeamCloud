/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorTeamCloudUserCreateCommand : OrchestratorCommand<UserDocument, OrchestratorTeamCloudUserCreateCommandResult>
    {
        public OrchestratorTeamCloudUserCreateCommand(UserDocument user, UserDocument payload) : base(user, payload) { }
    }
}
