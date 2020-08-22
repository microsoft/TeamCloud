/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTypeUpdateCommand : OrchestratorCommand<ProjectTypeDocument, OrchestratorProjectTypeUpdateCommandResult>
    {
        public OrchestratorProjectTypeUpdateCommand(UserDocument user, ProjectTypeDocument payload) : base(user, payload)
        { }
    }
}
