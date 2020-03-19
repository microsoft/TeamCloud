/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Commands
{
    public static class CommandExtensions
    {
        public static string StatusUrl(this ICommandResult result)
            => (result ?? throw new ArgumentNullException(nameof(result))).Links.TryGetValue("status", out var statusUrl) ? statusUrl : null;
    }
}
