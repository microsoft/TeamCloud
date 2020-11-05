/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorOrganizationDeleteCommand : OrchestratorDeleteCommand<OrganizationDocument, OrchestratorOrganizationDeleteCommandResult>
    {
        public OrchestratorOrganizationDeleteCommand(UserDocument user, OrganizationDocument payload) : base(user, payload)
        { }
    }
}
