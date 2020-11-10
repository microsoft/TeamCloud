/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentTemplateCreateCommand : OrchestratorCreateCommand<ComponentTemplate, OrchestratorComponentTemplateCreateCommandResult>
    {
        public OrchestratorComponentTemplateCreateCommand(User user, ComponentTemplate payload) : base(user, payload) { }
    }
}
