/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model
{
    public static class GlobalExtensions
    {
        public static IDisposable BeginProjectScope(this ILogger logger, Project project)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (project is null)
                throw new ArgumentNullException(nameof(project));

            return logger.BeginScope(new Dictionary<string, object>()
            {
                { "projectId", project.Id },
                { "projectName", project.Name }
            });
        }

        public static IDisposable BeginCommandScope(this ILogger logger, ICommand command, Provider provider = default)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var project = command.Payload as Project;

            return logger.BeginScope(new Dictionary<string, object>()
            {
                { "commandId", command.CommandId },
                { "commandType", command.GetType().Name },
                { "projectId", project?.Id ?? command.ProjectId.GetValueOrDefault().ToString() },
                { "projectName", project?.Name },
                { "providerId", provider?.Id }
            });
        }
    }
}
