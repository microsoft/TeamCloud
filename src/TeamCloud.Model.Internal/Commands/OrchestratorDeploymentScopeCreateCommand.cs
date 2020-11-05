/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorDeploymentScopeCreateCommand : OrchestratorCreateCommand<DeploymentScopeDocument, OrchestratorDeploymentScopeCreateCommandResult>
    {
        public OrchestratorDeploymentScopeCreateCommand(UserDocument user, DeploymentScopeDocument payload) : base(user, payload)
        { }
    }
}
