/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using TeamCloud.Model.Commands.Core;
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
    }
}
