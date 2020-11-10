
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
    public static class OrchestratorExtensions
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
                    commandResult.Links.Add("status", new Uri(baseUrl, $"api/{commandResult.OrganizationId}/status/{commandResult.CommandId}").ToString());
                else
                    commandResult.Links.Add("status", new Uri(baseUrl, $"api/{commandResult.OrganizationId}/projects/{projectId}/status/{commandResult.CommandId}").ToString());
            }

            if (commandResult.CommandAction == CommandAction.Delete)
                return commandResult; // delete commands don't provide a status location endpoint

            var locationPath = commandResult.GetLocationPath(projectId);

            if (!string.IsNullOrEmpty(locationPath))
                commandResult.Links.Add("location", new Uri(baseUrl, locationPath).ToString());

            return commandResult;
        }

        public static string GetLocationPath(this ICommandResult commandResult, string projectId)
            => commandResult switch
            {
                ICommandResult result
                    when result.Result is null
                    => null,
                ICommandResult result
                    when string.IsNullOrEmpty((result.Result as IIdentifiable)?.Id)
                    => null,
                ICommandResult<TeamCloudInstance> _
                    => $"api/{commandResult.OrganizationId}/admin/teamCloudInstance",
                ICommandResult<Project> result
                    => $"api/{commandResult.OrganizationId}/projects/{result.Result.Id}",
                ICommandResult<ProjectTemplate> result
                    => $"api/{commandResult.OrganizationId}/templates/{result.Result.Id}",
                ICommandResult<DeploymentScope> result
                    => $"api/{commandResult.OrganizationId}/scopes/{result.Result.Id}",
                ICommandResult<User> result
                    when !string.IsNullOrEmpty(projectId)
                    => $"api/{commandResult.OrganizationId}/projects/{projectId}/users/{result.Result.Id}",
                ICommandResult<User> result
                    => $"api/{commandResult.OrganizationId}/users/{result.Result.Id}",
                // ICommandResult<ProviderData> result
                //     when !string.IsNullOrEmpty(projectId)
                //     => !string.IsNullOrEmpty(result.Result.ProviderId)
                //      ? $"api/{commandResult.OrganizationId}/projects/{projectId}/providers/{result.Result.ProviderId}/data/{result.Result.Id}"
                //      : throw new InvalidOperationException("ProviderData must have a value for ProviderId to create location url."),
                // ICommandResult<ProviderData> result
                //     => !string.IsNullOrEmpty(result.Result.ProviderId)
                //      ? $"api/{commandResult.OrganizationId}/providers/{result.Result.ProviderId}/data/{result.Result.Id}"
                //      : throw new InvalidOperationException("ProviderData must have a value for ProviderId to create location url."),
                ICommandResult<ProjectLink> result
                    => !string.IsNullOrEmpty(projectId ?? result.Result.ProjectId)
                     ? $"api/{commandResult.OrganizationId}/projects/{projectId ?? result.Result.ProjectId}/links/{result.Result.Id}"
                     : throw new InvalidOperationException("ProjectLink must have a value for ProjectId to create location url."),
                ICommandResult<ComponentOffer> result
                    => !string.IsNullOrEmpty(result.Result.ProviderId)
                     ? $"api/{commandResult.OrganizationId}/providers/{result.Result.ProviderId}/offers/{result.Result.Id}"
                     : throw new InvalidOperationException("ComponentOffer must have a value for providerId to create location url."),
                ICommandResult<Component> result
                    => !string.IsNullOrEmpty(projectId ?? result.Result.ProjectId)
                     ? $"api/{commandResult.OrganizationId}/projects/{projectId ?? result.Result.ProjectId}/componenets/{result.Result.Id}"
                     : throw new InvalidOperationException("Component must have a value for ProjectId to create location url."),
                _ => null
            };
    }
}
