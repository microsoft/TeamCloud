/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentDeleteCommand : OrchestratorDeleteCommand<Component, OrchestratorComponentDeleteCommandResult>
    {
        public OrchestratorComponentDeleteCommand(User user, Component payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
