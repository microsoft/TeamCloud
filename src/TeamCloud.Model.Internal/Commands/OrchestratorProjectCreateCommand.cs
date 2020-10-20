/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectCreateCommand : OrchestratorCreateCommand<ProjectDocument, OrchestratorProjectCreateCommandResult, ProviderProjectCreateCommand, Project>
    {
        public OrchestratorProjectCreateCommand(UserDocument user, ProjectDocument payload) : base(user, payload)
        { }
    }
}
