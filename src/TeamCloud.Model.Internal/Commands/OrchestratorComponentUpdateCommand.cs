/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentUpdateCommand : OrchestratorUpdateCommand<ComponentDocument, OrchestratorComponentUpdateCommandResult>
    {
        public OrchestratorComponentUpdateCommand(UserDocument user, ComponentDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
