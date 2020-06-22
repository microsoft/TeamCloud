/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectDeleteCommand : OrchestratorCommand<Project, OrchestratorProjectDeleteCommandResult, ProviderProjectDeleteCommand, Model.Data.Project>
    {
        public OrchestratorProjectDeleteCommand(User user, Project payload) : base(user, payload) { }
    }
}
