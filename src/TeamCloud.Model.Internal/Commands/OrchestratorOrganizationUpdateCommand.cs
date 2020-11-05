/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorOrganizationUpdateCommand : OrchestratorUpdateCommand<OrganizationDocument, OrchestratorOrganizationUpdateCommandResult>
    {
        public OrchestratorOrganizationUpdateCommand(UserDocument user, OrganizationDocument payload) : base(user, payload)
        { }
    }
}
