/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTypeCreateCommand : OrchestratorCommand<ProjectTypeDocument, OrchestratorProjectTypeCreateCommandResult>
    {
        public OrchestratorProjectTypeCreateCommand(UserDocument user, ProjectTypeDocument payload) : base(user, payload)
        { }
    }
}
