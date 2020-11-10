/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentTemplateDeleteCommand : OrchestratorDeleteCommand<ComponentTemplate, OrchestratorComponentTemplateDeleteCommandResult>
    {
        public OrchestratorComponentTemplateDeleteCommand(User user, ComponentTemplate payload) : base(user, payload) { }
    }
}
