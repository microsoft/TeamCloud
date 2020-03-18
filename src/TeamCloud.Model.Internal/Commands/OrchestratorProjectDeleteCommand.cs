/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectDeleteCommand : OrchestratorCommand<Project, OrchestratorProjectDeleteCommandResult, ProviderProjectDeleteCommand>
    {
        public OrchestratorProjectDeleteCommand(User user, Project payload) : base(user, payload) { }
    }
}
