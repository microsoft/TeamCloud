/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public sealed class DeploymentScopeCreateCommand : CreateCommand<DeploymentScope, DeploymentScopeCreateCommandResult>
{
    public DeploymentScopeCreateCommand(User user, DeploymentScope payload)
        : base(user, payload)
    { }
}