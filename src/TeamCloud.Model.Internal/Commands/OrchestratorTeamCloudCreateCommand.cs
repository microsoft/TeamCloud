/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorTeamCloudCreateCommand : OrchestratorCommand<TeamCloudConfiguration, OrchestratorTeamCloudCreateCommandResult>
    {
        public OrchestratorTeamCloudCreateCommand(User user, TeamCloudConfiguration payload) : base(user, payload) { }
    }
}
