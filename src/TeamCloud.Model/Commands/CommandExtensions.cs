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
    public static class CommandExtensions
    {
        private static readonly CommandRuntimeStatus[] FinalRuntimeStatus = new CommandRuntimeStatus[]
        {
            CommandRuntimeStatus.Canceled,
            CommandRuntimeStatus.Completed,
            CommandRuntimeStatus.Terminated
        };

        public static bool IsFinal(this CommandRuntimeStatus status)
            => FinalRuntimeStatus.Contains(status);

        public static string StatusUrl(this ICommandResult result)
            => result.Links.TryGetValue("status", out var statusUrl) ? statusUrl : null;

        public static IEnumerable<ProviderCommandMessage> GetProviderCommandMessages(this TeamCloudInstance teamCloud, ICommand command)
            => teamCloud.Providers.Select(provider => new ProviderCommandMessage { Provider = provider, Command = command });
    }
}
