/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectCreateCommand : OrchestratorCommand<Project, OrchestratorProjectCreateCommandResult, ProviderProjectCreateCommand, Model.Data.Project>
    {
        public OrchestratorProjectCreateCommand(User user, Project payload) : base(user, payload)
        { }
    }
}
