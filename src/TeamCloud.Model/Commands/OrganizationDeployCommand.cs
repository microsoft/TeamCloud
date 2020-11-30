/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrganizationDeployCommand : DeployCommand<Organization, OrganizationDeployCommandResult>
    {
        public OrganizationDeployCommand(User user, Organization payload) : base(user, payload)
        { }
    }
}
