/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorDeploymentScopeUpdateCommand : OrchestratorUpdateCommand<DeploymentScope, OrchestratorDeploymentScopeUpdateCommandResult>
    {
        public OrchestratorDeploymentScopeUpdateCommand(User user, DeploymentScope payload) : base(user, payload)
        { }
    }
}
