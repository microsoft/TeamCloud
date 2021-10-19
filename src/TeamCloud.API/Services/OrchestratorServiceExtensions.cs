/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services
{
    public static class OrchestratorServiceExtensions
    {
        public static ICommandResult SetResultLinks(this ICommandResult commandResult, IHttpContextAccessor httpContextAccessor, HttpResponseMessage commandResponse, string projectId)
        {
            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            if (httpContextAccessor is null)
                throw new ArgumentNullException(nameof(httpContextAccessor));

            if (commandResponse is null)
                throw new ArgumentNullException(nameof(commandResponse));

            var baseUrl = httpContextAccessor.HttpContext?.GetApplicationBaseUrl(true);

            if (baseUrl is null)
                return commandResult; // as we couldn't resolve a base url, we can't generate status or location urls for our response object

            if (commandResponse.StatusCode == HttpStatusCode.Accepted)
            {
                if (string.IsNullOrEmpty(projectId))
                    commandResult.Links.Add("status", new Uri(baseUrl, $"orgs/{commandResult.OrganizationId}/status/{commandResult.CommandId}").ToString());
                else
                    commandResult.Links.Add("status", new Uri(baseUrl, $"orgs/{commandResult.OrganizationId}/projects/{projectId}/status/{commandResult.CommandId}").ToString());
            }

            if (commandResult.CommandAction == CommandAction.Delete)
                return commandResult; // delete commands don't provide a status location endpoint

            var locationPath = commandResult.GetLocationPath(commandResult.OrganizationId, projectId);

            if (!string.IsNullOrEmpty(locationPath))
                commandResult.Links.Add("location", new Uri(baseUrl, locationPath).ToString());

            return commandResult;
        }

        public static string GetLocationPath(this ICommandResult commandResult, string org, string projectId)
            => commandResult switch
            {
                ICommandResult result
                    when result.Result is null
                    => null,
                ICommandResult<Organization> result
                    => $"orgs/{result.Result.Id}",
                ICommandResult result
                    when string.IsNullOrEmpty(org) || string.IsNullOrEmpty((result.Result as IIdentifiable)?.Id)
                    => null,
                ICommandResult<Project> result
                    => $"orgs/{org}/projects/{result.Result.Id}",
                ICommandResult<ProjectIdentity> result
                    => $"orgs/{org}/projects/{result.Result.ProjectId}/identities/{result.Result.Id}",
                ICommandResult<ProjectTemplate> result
                    => $"orgs/{org}/templates/{result.Result.Id}",
                ICommandResult<DeploymentScope> result
                    => $"orgs/{org}/scopes/{result.Result.Id}",
                ICommandResult<User> result
                    when !string.IsNullOrEmpty(projectId)
                    => $"orgs/{org}/projects/{projectId}/users/{result.Result.Id}",
                ICommandResult<User> result
                    => $"orgs/{org}/users/{result.Result.Id}",
                // ICommandResult<ProjectLink> result
                //     => !string.IsNullOrEmpty(projectId ?? result.Result.ProjectId)
                //      ? $"orgs/{org}/projects/{projectId ?? result.Result.ProjectId}/links/{result.Result.Id}"
                //      : throw new InvalidOperationException("ProjectLink must have a value for ProjectId to create location url."),
                ICommandResult<ComponentTemplate> result
                    => !string.IsNullOrEmpty(projectId)
                     ? $"orgs/{org}/projects/{projectId}/templates/{result.Result.Id}"
                     : throw new InvalidOperationException("ComponentTemplate must have a value for ProjectId to create location url."),
                ICommandResult<Component> result
                    => !string.IsNullOrEmpty(projectId ?? result.Result.ProjectId)
                     ? $"orgs/{org}/projects/{projectId ?? result.Result.ProjectId}/components/{result.Result.Id}"
                     : throw new InvalidOperationException("Component must have a value for ProjectId to create location url."),
                ICommandResult<ComponentTask> result
                    => !string.IsNullOrEmpty(projectId ?? result.Result.ProjectId)
                     ? $"orgs/{org}/projects/{projectId ?? result.Result.ProjectId}/components/{result.Result.ComponentId}/tasks/{result.Result.Id}"
                     : throw new InvalidOperationException("Component task must have a value for ProjectId to create location url."),
                ICommandResult<Schedule> result
                    => !string.IsNullOrEmpty(projectId ?? result.Result.ProjectId)
                    ? $"orgs/{org}/projects/{projectId ?? result.Result.ProjectId}/schedules/{result.Result.Id}"
                    : throw new InvalidOperationException("Schedule must have a value for ProjectId to create location url."),
                _ => null
            };
    }
}
