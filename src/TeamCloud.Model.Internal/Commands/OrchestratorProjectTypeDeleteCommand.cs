/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTypeDeleteCommand : OrchestratorDeleteCommand<ProjectTypeDocument, OrchestratorProjectTypeDeleteCommandResult>
    {
        public OrchestratorProjectTypeDeleteCommand(UserDocument user, ProjectTypeDocument payload) : base(user, payload)
        { }
    }
}
