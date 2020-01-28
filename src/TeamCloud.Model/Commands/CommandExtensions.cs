/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;

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
    }
}
