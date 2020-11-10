/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class DeploymentScopeDeleteCommand : DeleteCommand<DeploymentScope, DeploymentScopeDeleteCommandResult>
    {
        public DeploymentScopeDeleteCommand(User user, DeploymentScope payload) : base(user, payload) { }
    }
}
