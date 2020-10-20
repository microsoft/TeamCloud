
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
                    commandResult.Links.Add("status", new Uri(baseUrl, $"api/status/{commandResult.CommandId}").ToString());
                else
                    commandResult.Links.Add("status", new Uri(baseUrl, $"api/projects/{projectId}/status/{commandResult.CommandId}").ToString());
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
                ICommandResult<TeamCloudInstanceDocument> _
                    => "api/admin/teamCloudInstance",
                ICommandResult<ProjectDocument> result
                    => $"api/projects/{result.Result.Id}",
                ICommandResult<ProviderDocument> result
                    => $"api/providers/{result.Result.Id}",
                ICommandResult<ProjectTypeDocument> result
                    => $"api/projectTypes/{result.Result.Id}",
                ICommandResult<UserDocument> result
                    when !string.IsNullOrEmpty(projectId)
                    => $"api/projects/{projectId}/users/{result.Result.Id}",
                ICommandResult<UserDocument> result
                    => $"api/users/{result.Result.Id}",
                ICommandResult<ProviderDataDocument> result
                    when !string.IsNullOrEmpty(projectId)
                    => !string.IsNullOrEmpty(result.Result.ProviderId)
                     ? $"api/projects/{projectId}/providers/{result.Result.ProviderId}/data/{result.Result.Id}"
                     : throw new InvalidOperationException("ProviderDataDocument must have a value for ProviderId to create location url."),
                ICommandResult<ProviderDataDocument> result
                    => !string.IsNullOrEmpty(result.Result.ProviderId)
                     ? $"api/providers/{result.Result.ProviderId}/data/{result.Result.Id}"
                     : throw new InvalidOperationException("ProviderDataDocument must have a value for ProviderId to create location url."),
                ICommandResult<ProjectLinkDocument> result
                    => !string.IsNullOrEmpty(projectId ?? result.Result.ProjectId)
                     ? $"api/projects/{projectId ?? result.Result.ProjectId}/links/{result.Result.Id}"
                     : throw new InvalidOperationException("ProjectLinkDocument must have a value for ProjectId to create location url."),
                ICommandResult<ComponentOfferDocument> result
                    => !string.IsNullOrEmpty(result.Result.ProviderId)
                     ? $"api/providers/{result.Result.ProviderId}/offers/{result.Result.Id}"
                     : throw new InvalidOperationException("ComponentOfferDocument must have a value for providerId to create location url."),
                ICommandResult<ComponentDocument> result
                    => !string.IsNullOrEmpty(projectId ?? result.Result.ProjectId)
                     ? $"api/projects/{projectId ?? result.Result.ProjectId}/componenets/{result.Result.Id}"
                     : throw new InvalidOperationException("ComponentDocument must have a value for ProjectId to create location url."),
                _ => null
            };
    }
}
