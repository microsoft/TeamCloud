/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTemplateCreateCommand : OrchestratorCreateCommand<ProjectTemplate, OrchestratorProjectTemplateCreateCommandResult>
    {
        public OrchestratorProjectTemplateCreateCommand(User user, ProjectTemplate payload) : base(user, payload)
        { }
    }
}
