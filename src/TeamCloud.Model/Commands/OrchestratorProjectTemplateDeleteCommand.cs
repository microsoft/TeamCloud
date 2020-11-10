/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTemplateDeleteCommand : OrchestratorDeleteCommand<ProjectTemplate, OrchestratorProjectTemplateDeleteCommandResult>
    {
        public OrchestratorProjectTemplateDeleteCommand(User user, ProjectTemplate payload) : base(user, payload)
        { }
    }
}
