/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public sealed class OrganizationUpdateCommand : UpdateCommand<Organization, OrganizationUpdateCommandResult>
{
    public OrganizationUpdateCommand(User user, Organization payload)
        : base(user, payload)
    { }
}
