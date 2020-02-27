/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public static class Extensions
    {
        private static readonly CommandRuntimeStatus[] FinalRuntimeStatus = new CommandRuntimeStatus[]
        {
            CommandRuntimeStatus.Canceled,
            CommandRuntimeStatus.Completed,
            CommandRuntimeStatus.Terminated,
            CommandRuntimeStatus.Failed
        };

        private static readonly CommandRuntimeStatus[] RunningRuntimeStatus = new CommandRuntimeStatus[]
        {
            CommandRuntimeStatus.Running,
            CommandRuntimeStatus.ContinuedAsNew,
            CommandRuntimeStatus.Pending
        };

        public static bool IsFinal(this CommandRuntimeStatus status)
            => FinalRuntimeStatus.Contains(status);

        public static bool IsRunning(this CommandRuntimeStatus status)
            => RunningRuntimeStatus.Contains(status);

        public static string StatusUrl(this ICommandResult result)
            => result.Links.TryGetValue("status", out var statusUrl) ? statusUrl : null;

        public static List<Provider> ProvidersFor(this TeamCloudInstance teamCloud, Project project)
            => teamCloud.Providers.Where(provider => project.Type.Providers.Any(p => p.Id == provider.Id)).ToList();

        public static ICommand GetProviderCommand(this ICommand command, Provider provider) => command switch
        {
            ProjectCreateCommand c => new ProviderProjectCreateCommand(c.CommandId, provider.Id, c.User, c.Payload),
            ProjectUpdateCommand c => new ProviderProjectUpdateCommand(c.CommandId, provider.Id, c.User, c.Payload),
            ProjectDeleteCommand c => new ProviderProjectDeleteCommand(c.CommandId, provider.Id, c.User, c.Payload),
            ProjectUserCreateCommand c => new ProviderProjectUserCreateCommand(c.CommandId, provider.Id, c.User, c.Payload, c.ProjectId.Value),
            ProjectUserUpdateCommand c => new ProviderProjectUserUpdateCommand(c.CommandId, provider.Id, c.User, c.Payload, c.ProjectId.Value),
            ProjectUserDeleteCommand c => new ProviderProjectUserDeleteCommand(c.CommandId, provider.Id, c.User, c.Payload, c.ProjectId.Value),
            TeamCloudUserCreateCommand c => new ProviderTeamCloudUserCreateCommand(c.CommandId, provider.Id, c.User, c.Payload),
            TeamCloudUserUpdateCommand c => new ProviderTeamCloudUserUpdateCommand(c.CommandId, provider.Id, c.User, c.Payload),
            TeamCloudUserDeleteCommand c => new ProviderTeamCloudUserDeleteCommand(c.CommandId, provider.Id, c.User, c.Payload),
            _ => throw new NotSupportedException()
        };

        public static string LocationPath(this ICommandResult commandResult, Guid? projectId) => commandResult switch
        {
            ProjectCreateCommandResult _ => $"api/projects/{projectId}",
            ProjectUpdateCommandResult _ => $"api/projects/{projectId}",
            ProjectUserCreateCommandResult result => PathEnsuringUserId(result.Result, $"api/projects/{projectId}/users/"),
            ProjectUserUpdateCommandResult result => PathEnsuringUserId(result.Result, $"api/projects/{projectId}/users/"),
            TeamCloudCreateCommandResult _ => $"api/config",
            TeamCloudUserCreateCommandResult result => PathEnsuringUserId(result.Result, $"api/users/"),
            TeamCloudUserUpdateCommandResult result => PathEnsuringUserId(result.Result, $"api/users/"),
            _ => null
        };

        private static string PathEnsuringUserId(User user, string path)
            => user == null || user.Id == Guid.Empty ? null : $"{path}{user.Id}";
    }
}
