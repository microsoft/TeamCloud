/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTemplateCreateCommand : OrchestratorCreateCommand<ProjectTemplateDocument, OrchestratorProjectTemplateCreateCommandResult>
    {
        public OrchestratorProjectTemplateCreateCommand(UserDocument user, ProjectTemplateDocument payload) : base(user, payload)
        { }
    }
}
