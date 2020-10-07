/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectLinkCreateCommand : OrchestratorCreateCommand<ProjectLinkDocument, OrchestratorProjectLinkCreateCommandResult>
    {
        public OrchestratorProjectLinkCreateCommand(UserDocument user, ProjectLinkDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
