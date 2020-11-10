/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentUpdateCommand : OrchestratorUpdateCommand<Component, OrchestratorComponentUpdateCommandResult>
    {
        public OrchestratorComponentUpdateCommand(User user, Component payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
