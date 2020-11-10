/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrganizationUserCreateCommand : CreateCommand<User, OrganizationUserCreateCommandResult>
    {
        public OrganizationUserCreateCommand(User user, User payload) : base(user, payload) { }
    }
}
