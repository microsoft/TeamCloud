/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectLinkUpdateCommand : OrchestratorUpdateCommand<ProjectLink, OrchestratorProjectLinkUpdateCommandResult>
    {
        public OrchestratorProjectLinkUpdateCommand(User user, ProjectLink payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
