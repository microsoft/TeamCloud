/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorTeamCloudInstanceSetCommand : OrchestratorCommand<TeamCloudInstanceDocument, OrchestratorTeamCloudInstanceSetCommandResult>
    {
        public OrchestratorTeamCloudInstanceSetCommand(UserDocument user, TeamCloudInstanceDocument payload) : base(user, payload)
        { }
    }
}
