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
                { "projectId", project?.Id ?? command.ProjectId },
                { "projectName", project?.Name },
                { "providerId", provider?.Id }
            });
        }

        public static void MergeTags(this ITags resource, IDictionary<string, string> tags, bool overwriteExistingValues = true)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (resource.Tags is null)
                resource.Tags = new Dictionary<string, string>();

            if (overwriteExistingValues)
            {
                tags.ToList().ForEach(t => resource.Tags[t.Key] = t.Value);
            }
            else
            {
                var keyValuePairs = resource.Tags
                                .Concat(tags);

                resource.Tags = keyValuePairs
                    .GroupBy(kvp => kvp.Key)
                    .Where(kvp => kvp.First().Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
            }
        }

        public static void MergeProperties(this IProperties resource, IDictionary<string, string> properties, bool overwriteExistingValues = true)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (resource.Properties is null)
                resource.Properties = new Dictionary<string, string>();

            if (overwriteExistingValues)
            {
                properties.ToList().ForEach(t => resource.Properties[t.Key] = t.Value);
            }
            else
            {
                var keyValuePairs = resource.Properties
                                .Concat(properties);

                resource.Properties = keyValuePairs
                    .GroupBy(kvp => kvp.Key)
                    .Where(kvp => kvp.First().Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
            }
        }
    }
}
