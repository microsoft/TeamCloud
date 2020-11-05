/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorOrganizationCreateCommand : OrchestratorCreateCommand<OrganizationDocument, OrchestratorOrganizationCreateCommandResult>
    {
        public OrchestratorOrganizationCreateCommand(UserDocument user, OrganizationDocument payload) : base(user, payload)
        { }
    }
}
