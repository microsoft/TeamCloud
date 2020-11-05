/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTemplateUpdateCommand : OrchestratorUpdateCommand<ProjectTemplateDocument, OrchestratorProjectTemplateUpdateCommandResult>
    {
        public OrchestratorProjectTemplateUpdateCommand(UserDocument user, ProjectTemplateDocument payload) : base(user, payload)
        { }
    }
}
