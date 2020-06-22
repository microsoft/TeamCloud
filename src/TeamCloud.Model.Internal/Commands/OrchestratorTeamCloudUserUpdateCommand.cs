/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorTeamCloudUserUpdateCommand : OrchestratorCommand<User, OrchestratorTeamCloudUserUpdateCommandResult>
    {
        public OrchestratorTeamCloudUserUpdateCommand(User user, User payload) : base(user, payload) { }
    }
}
