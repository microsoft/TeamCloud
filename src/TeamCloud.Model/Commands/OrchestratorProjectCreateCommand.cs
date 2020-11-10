/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectCreateCommand : OrchestratorCreateCommand<Project, OrchestratorProjectCreateCommandResult>
    {
        public OrchestratorProjectCreateCommand(User user, Project payload) : base(user, payload)
        { }
    }
}
