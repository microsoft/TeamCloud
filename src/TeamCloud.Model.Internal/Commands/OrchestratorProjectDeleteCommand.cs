/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectDeleteCommand : OrchestratorCommand<ProjectDocument, OrchestratorProjectDeleteCommandResult, ProviderProjectDeleteCommand, Model.Data.Project>
    {
        public OrchestratorProjectDeleteCommand(UserDocument user, ProjectDocument payload) : base(user, payload) { }
    }
}
