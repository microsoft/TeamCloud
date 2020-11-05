/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorDeploymentScopeDeleteCommand : OrchestratorDeleteCommand<DeploymentScopeDocument, OrchestratorDeploymentScopeDeleteCommandResult>
    {
        public OrchestratorDeploymentScopeDeleteCommand(UserDocument user, DeploymentScopeDocument payload) : base(user, payload)
        { }
    }
}
