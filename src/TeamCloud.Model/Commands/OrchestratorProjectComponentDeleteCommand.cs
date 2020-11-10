/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectComponentDeleteCommand : OrchestratorDeleteCommand<Component, OrchestratorComponentDeleteCommandResult>
    {
        public OrchestratorProjectComponentDeleteCommand(User user, Component payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
