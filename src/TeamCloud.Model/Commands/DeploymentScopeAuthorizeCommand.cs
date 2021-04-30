using System;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class DeploymentScopeAuthorizeCommand : CustomCommand<DeploymentScope, DeploymentScopeAuthorizeCommandResult>
    {
        public DeploymentScopeAuthorizeCommand(User user, DeploymentScope payload, Guid? commandId = null) : base(user, payload, commandId)
        { }
    }
}
