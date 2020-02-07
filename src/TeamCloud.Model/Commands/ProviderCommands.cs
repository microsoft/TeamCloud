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
        public override string ProviderId { get; set; }

        public ProviderRegisterCommand(Guid commandId, string providerId, User user, ProviderConfiguration payload) : base(commandId, user, payload)
            => ProviderId = providerId;
    }

    public class ProviderProjectCreateCommand : Command<Project, ProviderProjectCreateCommandResult>
    {
        public override Guid? ProjectId => Payload.Id;

        public override string ProviderId { get; set; }

        public ProviderProjectCreateCommand(Guid commandId, string providerId, User user, Project payload) : base(commandId, user, payload)
            => ProviderId = providerId;
    }

    public class ProviderProjectUpdateCommand : Command<Project, ProviderProjectUpdateCommandResult>
    {
        public override Guid? ProjectId => Payload.Id;

        public override string ProviderId { get; set; }

        public ProviderProjectUpdateCommand(Guid commandId, string providerId, User user, Project payload) : base(commandId, user, payload)
            => ProviderId = providerId;
    }

    public class ProviderProjectDeleteCommand : Command<Project, ProviderProjectDeleteCommandResult>
    {
        public override Guid? ProjectId => Payload.Id;

        public override string ProviderId { get; set; }

        public ProviderProjectDeleteCommand(Guid commandId, string providerId, User user, Project payload) : base(commandId, user, payload)
            => ProviderId = providerId;
    }

    public class ProviderProjectUserCreateCommand : Command<User, ProviderProjectUserCreateCommandResult>
    {
        public override Guid? ProjectId { get; set; }

        public override string ProviderId { get; set; }

        public ProviderProjectUserCreateCommand(Guid commandId, string providerId, User user, User payload, Guid projectId) : base(commandId, user, payload)
        {
            ProviderId = providerId;
            ProjectId = projectId;
        }
    }

    public class ProviderProjectUserUpdateCommand : Command<User, ProviderProjectUserUpdateCommandResult>
    {
        public override Guid? ProjectId { get; set; }

        public override string ProviderId { get; set; }

        public ProviderProjectUserUpdateCommand(Guid commandId, string providerId, User user, User payload, Guid projectId) : base(commandId, user, payload)
        {
            ProviderId = providerId;
            ProjectId = projectId;
        }
    }

    public class ProviderProjectUserDeleteCommand : Command<User, ProviderProjectUserDeleteCommandResult>
    {
        public override Guid? ProjectId { get; set; }

        public override string ProviderId { get; set; }

        public ProviderProjectUserDeleteCommand(Guid commandId, string providerId, User user, User payload, Guid projectId) : base(commandId, user, payload)
        {
            ProviderId = providerId;
            ProjectId = projectId;
        }
    }

    public class ProviderTeamCloudCreateCommand : Command<TeamCloudConfiguration, ProviderTeamCloudCreateCommandResult>
    {
        public override string ProviderId { get; set; }

        public ProviderTeamCloudCreateCommand(Guid commandId, string providerId, User user, TeamCloudConfiguration payload) : base(commandId, user, payload)
            => ProviderId = providerId;
    }

    public class ProviderTeamCloudUserCreateCommand : Command<User, ProviderTeamCloudUserCreateCommandResult>
    {
        public override string ProviderId { get; set; }

        public ProviderTeamCloudUserCreateCommand(Guid commandId, string providerId, User user, User payload) : base(commandId, user, payload)
            => ProviderId = providerId;
    }

    public class ProviderTeamCloudUserUpdateCommand : Command<User, ProviderTeamCloudUserUpdateCommandResult>
    {
        public override string ProviderId { get; set; }

        public ProviderTeamCloudUserUpdateCommand(Guid commandId, string providerId, User user, User payload) : base(commandId, user, payload)
            => ProviderId = providerId;
    }

    public class ProviderTeamCloudUserDeleteCommand : Command<User, ProviderTeamCloudUserDeleteCommandResult>
    {
        public override string ProviderId { get; set; }

        public ProviderTeamCloudUserDeleteCommand(Guid commandId, string providerId, User user, User payload) : base(commandId, user, payload)
            => ProviderId = providerId;
    }
}
