/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorOrganizationUserCreateCommand : OrchestratorCreateCommand<User, OrchestratorOrganizationUserCreateCommandResult>
    {
        public OrchestratorOrganizationUserCreateCommand(User user, User payload) : base(user, payload) { }
    }
}
