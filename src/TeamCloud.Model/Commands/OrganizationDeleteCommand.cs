/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public sealed class OrganizationDeleteCommand : DeleteCommand<Organization, OrganizationDeleteCommandResult>
{
    public OrganizationDeleteCommand(User user, Organization payload)
        : base(user, payload)
    { }
}
