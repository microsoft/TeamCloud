/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Broadcast;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model;

public static class Extensions
{
    public static T MapTo<T>(this Enum instance, T defaultValue, bool ignoreCase = false)
        where T : struct
    {
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));

        return Enum.TryParse<T>(Enum.GetName(instance.GetType(), instance), ignoreCase, out T value)
            ? value : defaultValue;
    }

    public static BroadcastMessage ToBroadcastMessage(this ICommandResult commandResult)
    {
        if (commandResult is null)
            throw new NullReferenceException();

        return new BroadcastMessage()
        {
            Action = commandResult.CommandAction.ToString().ToLowerInvariant(),
            Timestamp = commandResult.LastUpdatedTime.GetValueOrDefault(DateTime.UtcNow),
            Items = GetItems()
        };

        IEnumerable<BroadcastMessage.Item> GetItems()
        {
            if (commandResult.Result is IContainerDocument containerDocument)
                yield return containerDocument.ToBroadcastMessageItem();
            else
                throw new NotSupportedException($"Command results of type '{commandResult.GetType()}' cannot be converted into a broadcast message.");
        }
    }

    public static BroadcastMessage.Item ToBroadcastMessageItem(this IContainerDocument containerDocument) => containerDocument is null ? throw new NullReferenceException() : new BroadcastMessage.Item()
    {
        Id = containerDocument.Id,
        Type = containerDocument.GetType().Name.ToLowerInvariant(),
        Organization = (containerDocument as IProjectContext)?.Organization,
        Project = (containerDocument as IProjectContext)?.ProjectId,
        Component = (containerDocument as IComponentContext)?.ComponentId,
        ETag = containerDocument.ETag,
        Timestamp = containerDocument.Timestamp
    };

    public static string GetHubName(this Project project)
        => project is null ? throw new ArgumentNullException(nameof(project)) : GetHubName(project.Id);

    public static string GetHubName(this IProjectContext projectContext)
        => projectContext is null ? throw new ArgumentNullException(nameof(projectContext)) : GetHubName(projectContext.ProjectId);

    public static string GetHubName(this Organization organization)
        => organization is null ? throw new ArgumentNullException(nameof(organization)) : GetHubName(organization.Id);

    public static string GetHubName(this IOrganizationContext organizationContext)
        => organizationContext is null ? throw new ArgumentNullException(nameof(organizationContext)) : GetHubName(organizationContext.Organization);

    private static string GetHubName(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException($"'{nameof(identifier)}' cannot be null or whitespace.", nameof(identifier));

        var hubIdentifier = Guid.TryParse(identifier, out Guid guid)
            ? guid.ToString("N")
            : throw new ArgumentException($"Unable to create hub name from identifier '{identifier}'");

        return $"hub_{hubIdentifier}";
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
                .Where(kvp => kvp.First().Value is not null)
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
                .Where(kvp => kvp.First().Value is not null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
        }
    }

    public static Uri AppendPath(this Uri uri, string path)
    {
        if (uri is null)
            throw new ArgumentNullException(nameof(uri));

        if (string.IsNullOrEmpty(path))
            return uri;

        return new Uri(uri, path);
    }

    public static IDisposable BeginProjectScope(this ILogger logger, Project project)
    {
        if (logger is null)
            throw new ArgumentNullException(nameof(logger));

        if (project is null)
            throw new ArgumentNullException(nameof(project));

        return logger.BeginScope(new Dictionary<string, object>()
            {
                { "projectId", project.Id },
                { "projectName", project.DisplayName }
            });
    }

    public static IDisposable BeginCommandScope(this ILogger logger, ICommand command)
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
                { "projectName", project?.DisplayName }
            });
    }

    // public static IDisposable BeginProjectScope(this ILogger logger, Project project)
    // {
    //     if (logger is null)
    //         throw new ArgumentNullException(nameof(logger));

    //     if (project is null)
    //         throw new ArgumentNullException(nameof(project));

    //     return logger.BeginScope(new Dictionary<string, object>()
    //     {
    //         { "projectId", project.Id },
    //         { "projectName", project.Name }
    //     });
    // }

    // public static IDisposable BeginCommandScope(this ILogger logger, ICommand command, ProviderDocument provider = default)
    // {
    //     if (logger is null)
    //         throw new ArgumentNullException(nameof(logger));

    //     if (command is null)
    //         throw new ArgumentNullException(nameof(command));

    //     var project = command.Payload as Project;

    //     return logger.BeginScope(new Dictionary<string, object>()
    //     {
    //         { "commandId", command.CommandId },
    //         { "commandType", command.GetType().Name },
    //         { "projectId", project?.Id ?? command.ProjectId },
    //         { "projectName", project?.Name },
    //         { "providerId", provider?.Id }
    //     });
    // }
}
