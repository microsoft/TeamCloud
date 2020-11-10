/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorDeploymentScopeDeleteCommand : OrchestratorDeleteCommand<DeploymentScope, OrchestratorDeploymentScopeDeleteCommandResult>
    {
        public OrchestratorDeploymentScopeDeleteCommand(User user, DeploymentScope payload) : base(user, payload)
        { }
    }
}
