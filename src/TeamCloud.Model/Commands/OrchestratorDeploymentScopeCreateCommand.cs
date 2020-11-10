/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorDeploymentScopeCreateCommand : OrchestratorCreateCommand<DeploymentScope, OrchestratorDeploymentScopeCreateCommandResult>
    {
        public OrchestratorDeploymentScopeCreateCommand(User user, DeploymentScope payload) : base(user, payload)
        { }
    }
}
