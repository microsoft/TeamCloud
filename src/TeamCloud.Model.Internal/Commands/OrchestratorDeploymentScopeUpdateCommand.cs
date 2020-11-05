/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorDeploymentScopeUpdateCommand : OrchestratorUpdateCommand<DeploymentScopeDocument, OrchestratorDeploymentScopeUpdateCommandResult>
    {
        public OrchestratorDeploymentScopeUpdateCommand(UserDocument user, DeploymentScopeDocument payload) : base(user, payload)
        { }
    }
}
