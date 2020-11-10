/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorOrganizationDeleteCommand : OrchestratorDeleteCommand<Organization, OrchestratorOrganizationDeleteCommandResult>
    {
        public OrchestratorOrganizationDeleteCommand(User user, Organization payload) : base(user, payload)
        { }
    }
}
