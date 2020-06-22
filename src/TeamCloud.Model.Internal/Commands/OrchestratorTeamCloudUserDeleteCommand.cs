/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorTeamCloudUserDeleteCommand : OrchestratorCommand<User, OrchestratorTeamCloudUserDeleteCommandResult>
    {
        public OrchestratorTeamCloudUserDeleteCommand(User user, User payload) : base(user, payload) { }
    }
}
