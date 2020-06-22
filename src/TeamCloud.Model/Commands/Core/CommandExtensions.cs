/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Model.Commands.Core
{
    public static class CommandExtensions
    {
        public static string StatusUrl(this ICommandResult result)
            => (result ?? throw new ArgumentNullException(nameof(result))).Links.TryGetValue("status", out var statusUrl) ? statusUrl : null;
    }
}
