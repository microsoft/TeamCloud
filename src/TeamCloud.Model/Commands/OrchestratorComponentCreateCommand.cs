/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentCreateCommand : OrchestratorCreateCommand<Component, OrchestratorComponentCreateCommandResult>
    {
        public OrchestratorComponentCreateCommand(User user, Component payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
