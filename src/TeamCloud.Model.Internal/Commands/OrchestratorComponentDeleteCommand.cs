/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentDeleteCommand : OrchestratorCommand<ComponentDocument, OrchestratorComponentDeleteCommandResult>
    {
        public OrchestratorComponentDeleteCommand(UserDocument user, ComponentDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
