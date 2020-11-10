/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorOrganizationUserUpdateCommand : OrchestratorUpdateCommand<User, OrchestratorOrganizationUserUpdateCommandResult>
    {
        public OrchestratorOrganizationUserUpdateCommand(User user, User payload) : base(user, payload) { }
    }
}
