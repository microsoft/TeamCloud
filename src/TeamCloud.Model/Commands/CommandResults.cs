/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProjectCreateCommandResult : CommandResult<Project>
    {
        public ProjectCreateCommandResult() { }

        public ProjectCreateCommandResult(Guid commandId, Project result) : base(commandId)
            => Result = result;
    }


    public class ProjectUpdateCommandResult : CommandResult<Project>
    {
        public ProjectUpdateCommandResult() { }

        public ProjectUpdateCommandResult(Guid commandId, Project result) : base(commandId)
            => Result = result;
    }


    public class ProjectDeleteCommandResult : CommandResult<Project>
    {
        public ProjectDeleteCommandResult() { }

        public ProjectDeleteCommandResult(Guid commandId, Project result) : base(commandId)
            => Result = result;
    }


    public class ProjectUserCreateCommandResult : CommandResult<User>
    {
        public ProjectUserCreateCommandResult() { }

        public ProjectUserCreateCommandResult(Guid commandId, User result) : base(commandId)
            => Result = result;
    }


    public class ProjectUserUpdateCommandResult : CommandResult<User>
    {
        public ProjectUserUpdateCommandResult() { }

        public ProjectUserUpdateCommandResult(Guid commandId, User result) : base(commandId)
            => Result = result;
    }


    public class ProjectUserDeleteCommandResult : CommandResult<User>
    {
        public ProjectUserDeleteCommandResult() { }

        public ProjectUserDeleteCommandResult(Guid commandId, User result) : base(commandId)
            => Result = result;
    }


    public class TeamCloudCreateCommandResult : CommandResult<TeamCloudInstance>
    {
        public TeamCloudCreateCommandResult() { }

        public TeamCloudCreateCommandResult(Guid commandId, TeamCloudInstance result) : base(commandId)
            => Result = result;
    }


    public class TeamCloudUserCreateCommandResult : CommandResult<User>
    {
        public TeamCloudUserCreateCommandResult() { }

        public TeamCloudUserCreateCommandResult(Guid commandId, User result) : base(commandId)
            => Result = result;
    }


    public class TeamCloudUserUpdateCommandResult : CommandResult<User>
    {
        public TeamCloudUserUpdateCommandResult() { }

        public TeamCloudUserUpdateCommandResult(Guid commandId, User result) : base(commandId)
            => Result = result;
    }


    public class TeamCloudUserDeleteCommandResult : CommandResult<User>
    {
        public TeamCloudUserDeleteCommandResult() { }

        public TeamCloudUserDeleteCommandResult(Guid commandId, User result) : base(commandId)
            => Result = result;
    }
}
