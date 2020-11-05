/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTemplateDeleteCommand : OrchestratorDeleteCommand<ProjectTemplateDocument, OrchestratorProjectTemplateDeleteCommandResult>
    {
        public OrchestratorProjectTemplateDeleteCommand(UserDocument user, ProjectTemplateDocument payload) : base(user, payload)
        { }
    }
}
