//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;
public sealed class OrganizationPortalUpdateCommand : CustomCommand<Organization, OrganizationPortalUpdateCommandResult>
{
    public OrganizationPortalUpdateCommand(User user, Organization payload) : base(user, payload)
    { }
}
