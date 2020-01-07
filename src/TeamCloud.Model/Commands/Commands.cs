/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Model
{
    public class ProjectCreateCommand : Command<Project, Project>
    {
        public override Guid? ProjectId => Payload.Id;

        public ProjectCreateCommand(Project project) : base(project)
        { }
    }


    public class ProjectUpdateCommand : Command<Project, Project>
    {
        public override Guid? ProjectId => Payload.Id;

        public ProjectUpdateCommand(Project payload) : base(payload)
        {
        }
    }


    public class ProjectDeleteCommand : Command<ProjectDefinition, Project>
    {
        public override Guid? ProjectId => Payload.Id;

        public ProjectDeleteCommand(ProjectDefinition payload) : base(payload)
        {
        }
    }


    public class ProjectUserCreateCommand : Command<UserDefinition, User>
    {
        public override Guid? ProjectId { get; set; }

        public ProjectUserCreateCommand(UserDefinition payload, Guid projectId) : base(payload)
        {
            ProjectId = projectId;
        }
    }


    public class ProjectUserUpdateCommand : Command<User, Project>
    {
        public override Guid? ProjectId { get; set; }

        public ProjectUserUpdateCommand(User payload, Guid projectId) : base(payload)
        {
            ProjectId = projectId;
        }
    }


    public class ProjectUserDeleteCommand : Command<User, Project>
    {
        public override Guid? ProjectId { get; set; }

        public ProjectUserDeleteCommand(User payload, Guid projectId) : base(payload)
        {
            ProjectId = projectId;
        }
    }


    public class TeamCloudUserCreateCommand : Command<UserDefinition, User>
    {
        public TeamCloudUserCreateCommand(UserDefinition payload) : base(payload)
        {
        }
    }


    public class TeamCloudUserUpdateCommand : Command<User, User>
    {
        public TeamCloudUserUpdateCommand(User payload) : base(payload)
        {
        }
    }


    public class TeamCloudUserDeletCommand : Command<User, User>
    {
        public TeamCloudUserDeletCommand(User payload) : base(payload)
        {
        }
    }
}
