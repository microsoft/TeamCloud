/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectComponentCreateCommand : OrchestratorCommand<ComponentDocument, OrchestratorComponentCreateCommandResult>
    {
        public OrchestratorProjectComponentCreateCommand(UserDocument user, ComponentDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
