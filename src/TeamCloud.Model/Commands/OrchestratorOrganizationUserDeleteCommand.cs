/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorOrganizationUserDeleteCommand : OrchestratorDeleteCommand<User, OrchestratorOrganizationUserDeleteCommandResult>
    {
        public OrchestratorOrganizationUserDeleteCommand(User user, User payload) : base(user, payload) { }
    }
}
