/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentCreateCommand : OrchestratorCreateCommand<ComponentDocument, OrchestratorComponentCreateCommandResult>
    {
        public OrchestratorComponentCreateCommand(UserDocument user, ComponentDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
