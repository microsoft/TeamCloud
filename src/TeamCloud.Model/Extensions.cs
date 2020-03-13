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
        public static string StatusUrl(this ICommandResult result)
            => result.Links.TryGetValue("status", out var statusUrl) ? statusUrl : null;

        public static List<Provider> ProvidersFor(this TeamCloudInstance teamCloud, Project project)
            => teamCloud.Providers.Where(provider => project.Type.Providers.Any(p => p.Id == provider.Id)).ToList();
    }
}
