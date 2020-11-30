/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProjectDeployCommand : DeployCommand<Project, ProjectDeployCommandResult>
    {
        public ProjectDeployCommand(User user, Project payload) : base(user, payload) { }
    }
}
