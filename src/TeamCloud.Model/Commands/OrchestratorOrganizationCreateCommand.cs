/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorOrganizationCreateCommand : OrchestratorCreateCommand<Organization, OrchestratorOrganizationCreateCommandResult>
    {
        public OrchestratorOrganizationCreateCommand(User user, Organization payload) : base(user, payload)
        { }
    }
}
