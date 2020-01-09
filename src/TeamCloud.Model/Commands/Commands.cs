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

        public ProjectCreateCommand(User user, Project project) : base(user, project)
        {
        }
    }


    public class ProjectUpdateCommand : Command<Project, Project>
    {
        public override Guid? ProjectId => Payload.Id;

        public ProjectUpdateCommand(User user, Project payload) : base(user, payload)
        {
        }
    }


    public class ProjectDeleteCommand : Command<Project, Project>
    {
        public override Guid? ProjectId => Payload.Id;

        public ProjectDeleteCommand(User user, Project payload) : base(user, payload)
        {
        }
    }


    public class ProjectUserCreateCommand : Command<User, User>
    {
        public override Guid? ProjectId { get; set; }

        public ProjectUserCreateCommand(User user, User payload, Guid projectId) : base(user, payload)
        {
            ProjectId = projectId;
        }
    }


    public class ProjectUserUpdateCommand : Command<User, Project>
    {
        public override Guid? ProjectId { get; set; }

        public ProjectUserUpdateCommand(User user, User payload, Guid projectId) : base(user, payload)
        {
            ProjectId = projectId;
        }
    }


    public class ProjectUserDeleteCommand : Command<User, Project>
    {
        public override Guid? ProjectId { get; set; }

        public ProjectUserDeleteCommand(User user, User payload, Guid projectId) : base(user, payload)
        {
            ProjectId = projectId;
        }
    }


    public class TeamCloudUserCreateCommand : Command<User, User>
    {
        public TeamCloudUserCreateCommand(User user, User payload) : base(user, payload)
        {
        }
    }


    public class TeamCloudUserUpdateCommand : Command<User, User>
    {
        public TeamCloudUserUpdateCommand(User user, User payload) : base(user, payload)
        {
        }
    }


    public class TeamCloudUserDeletCommand : Command<User, User>
    {
        public TeamCloudUserDeletCommand(User user, User payload) : base(user, payload)
        {
        }
    }
}
