/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectTemplateUpdateCommand : OrchestratorUpdateCommand<ProjectTemplate, OrchestratorProjectTemplateUpdateCommandResult>
    {
        public OrchestratorProjectTemplateUpdateCommand(User user, ProjectTemplate payload) : base(user, payload)
        { }
    }
}
