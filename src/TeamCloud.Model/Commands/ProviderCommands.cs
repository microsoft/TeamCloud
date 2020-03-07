/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderRegisterCommand : Command<ProviderConfiguration, ProviderRegisterCommandResult>
    {
        public ProviderRegisterCommand(Guid commandId, User user, ProviderConfiguration payload) : base(commandId, user, payload)
        { }
    }

    public class ProviderProjectCreateCommand : Command<Project, ProviderProjectCreateCommandResult>
    {
        public ProviderProjectCreateCommand(Guid commandId, User user, Project payload) : base(commandId, user, payload)
        { }
    }

    public class ProviderProjectUpdateCommand : Command<Project, ProviderProjectUpdateCommandResult>
    {
        public ProviderProjectUpdateCommand(Guid commandId, User user, Project payload) : base(commandId, user, payload)
        { }
    }

    public class ProviderProjectDeleteCommand : Command<Project, ProviderProjectDeleteCommandResult>
    {
        public ProviderProjectDeleteCommand(Guid commandId, User user, Project payload) : base(commandId, user, payload)
        { }
    }

    public class ProviderProjectUserCreateCommand : Command<User, ProviderProjectUserCreateCommandResult>
    {
        public ProviderProjectUserCreateCommand(Guid commandId, User user, User payload, Guid projectId) : base(commandId, user, payload)
            => base.ProjectId = projectId;
    }

    public class ProviderProjectUserUpdateCommand : Command<User, ProviderProjectUserUpdateCommandResult>
    {
        public ProviderProjectUserUpdateCommand(Guid commandId, User user, User payload, Guid projectId) : base(commandId, user, payload)
            => base.ProjectId = projectId;
    }

    public class ProviderProjectUserDeleteCommand : Command<User, ProviderProjectUserDeleteCommandResult>
    {
        public ProviderProjectUserDeleteCommand(Guid commandId, User user, User payload, Guid projectId) : base(commandId, user, payload)
            => base.ProjectId = projectId;
    }

    public class ProviderTeamCloudUserCreateCommand : Command<User, ProviderTeamCloudUserCreateCommandResult>
    {
        public ProviderTeamCloudUserCreateCommand(Guid commandId, User user, User payload) : base(commandId, user, payload)
        { }
    }

    public class ProviderTeamCloudUserUpdateCommand : Command<User, ProviderTeamCloudUserUpdateCommandResult>
    {
        public ProviderTeamCloudUserUpdateCommand(Guid commandId, User user, User payload) : base(commandId, user, payload)
        { }
    }

    public class ProviderTeamCloudUserDeleteCommand : Command<User, ProviderTeamCloudUserDeleteCommandResult>
    {
        public ProviderTeamCloudUserDeleteCommand(Guid commandId, User user, User payload) : base(commandId, user, payload)
        { }
    }
}
