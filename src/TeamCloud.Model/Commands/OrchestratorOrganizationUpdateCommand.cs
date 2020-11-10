/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorOrganizationUpdateCommand : OrchestratorUpdateCommand<Organization, OrchestratorOrganizationUpdateCommandResult>
    {
        public OrchestratorOrganizationUpdateCommand(User user, Organization payload) : base(user, payload)
        { }
    }
}
