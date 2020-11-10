/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentTemplateUpdateCommand : OrchestratorUpdateCommand<ComponentTemplate, OrchestratorComponentTemplateUpdateCommandResult>
    {
        public OrchestratorComponentTemplateUpdateCommand(User user, ComponentTemplate payload) : base(user, payload) { }
    }
}
