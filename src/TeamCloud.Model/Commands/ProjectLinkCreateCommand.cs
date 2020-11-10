/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ProjectLinkCreateCommand : CreateCommand<ProjectLink, ProjectLinkCreateCommandResult>
    {
        public ProjectLinkCreateCommand(User user, ProjectLink payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
