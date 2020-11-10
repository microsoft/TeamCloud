/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorTeamCloudInstanceSetCommand : OrchestratorUpdateCommand<TeamCloudInstance, OrchestratorTeamCloudInstanceSetCommandResult>
    {
        public OrchestratorTeamCloudInstanceSetCommand(User user, TeamCloudInstance payload) : base(user, payload)
        { }
    }
}
