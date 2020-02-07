/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProjectCreateCommandResult : CommandResult<Project> { }

    public class ProjectUpdateCommandResult : CommandResult<Project> { }

    public class ProjectDeleteCommandResult : CommandResult<Project> { }

    public class ProjectUserCreateCommandResult : CommandResult<User> { }

    public class ProjectUserUpdateCommandResult : CommandResult<User> { }

    public class ProjectUserDeleteCommandResult : CommandResult<User> { }

    public class TeamCloudCreateCommandResult : CommandResult<TeamCloudInstance> { }

    public class TeamCloudUserCreateCommandResult : CommandResult<User> { }

    public class TeamCloudUserUpdateCommandResult : CommandResult<User> { }

    public class TeamCloudUserDeleteCommandResult : CommandResult<User> { }
}
