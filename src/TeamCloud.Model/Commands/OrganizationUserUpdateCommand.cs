/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public class OrganizationUserUpdateCommand : UpdateCommand<User, OrganizationUserUpdateCommandResult>
{
    public OrganizationUserUpdateCommand(User user, User payload)
        : base(user, payload)
    { }
}
