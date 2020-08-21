/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectLinkDeleteCommand : OrchestratorCommand<ProjectLinkDocument, OrchestratorProjectLinkDeleteCommandResult>
    {
        public OrchestratorProjectLinkDeleteCommand(UserDocument user, ProjectLinkDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
