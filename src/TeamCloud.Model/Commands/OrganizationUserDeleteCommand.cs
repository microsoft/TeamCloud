/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrganizationUserDeleteCommand : DeleteCommand<User, OrganizationUserDeleteCommandResult>
    {
        public OrganizationUserDeleteCommand(User user, User payload) : base(user, payload) { }
    }
}
