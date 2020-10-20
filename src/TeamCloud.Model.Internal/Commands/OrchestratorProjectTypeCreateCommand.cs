/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTypeCreateCommand : OrchestratorCreateCommand<ProjectTypeDocument, OrchestratorProjectTypeCreateCommandResult>
    {
        public OrchestratorProjectTypeCreateCommand(UserDocument user, ProjectTypeDocument payload) : base(user, payload)
        { }
    }
}
