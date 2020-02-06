/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services
{
    public interface IOrchestratorOptions
    {
        public string Url { get; set; }

        public string AuthCode { get; set; }
    }

    public class Orchestrator
    {
        private readonly IOrchestratorOptions options;
        private readonly IHttpContextAccessor httpContextAccessor;

        public Orchestrator(IOrchestratorOptions options, IHttpContextAccessor httpContextAccessor)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private void SetResultLinks(ICommandResult commandResult, Guid? projectId)
        {
            var baseUrl = httpContextAccessor.HttpContext.GetApplicationBaseUrl();

            if (projectId.HasValue && !projectId.Value.Equals(Guid.Empty))
            {
                commandResult.Links.Add("status", new Uri(baseUrl, $"api/projects/{projectId}/status/{commandResult.CommandId}").ToString());
                commandResult.Links.Add("project", new Uri(baseUrl, $"api/projects/{projectId}").ToString());
            }
            else
            {
                commandResult.Links.Add("status", new Uri(baseUrl, $"api/status/{commandResult.CommandId}").ToString());
            }

            commandResult.Links.Add("location", new Uri(baseUrl, GetLocation(commandResult, projectId)).ToString());
        }

        private string GetLocation(ICommandResult commandResult, Guid? projectId) => (commandResult) switch
        {
            ProjectCreateCommandResult _ => $"api/projects/{projectId}",
            ProjectUpdateCommandResult _ => $"api/projects/{projectId}",
            ProjectUserCreateCommandResult result => $"api/projects/{projectId}/users/{result.Result.Id}",
            ProjectUserUpdateCommandResult result => $"api/projects/{projectId}/users/{result.Result.Id}",
            TeamCloudCreateCommandResult _ => $"api/config",
            TeamCloudUserCreateCommandResult result => $"api/users/{result.Result.Id}",
            TeamCloudUserUpdateCommandResult result => $"api/users/{result.Result.Id}",
            _ => null
        };


        public async Task<ICommandResult> QueryAsync(Guid commandId, Guid? projectId)
        {
            var resultJson = await options.Url
                .AppendPathSegment($"api/command/{commandId}")
                .WithHeader("x-functions-key", options.AuthCode)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .GetStringAsync()
                .ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<ICommandResult>(resultJson);

            SetResultLinks(result, projectId);

            return result;
        }

        public async Task<ICommandResult> InvokeAsync(ICommand command)
        {
            var commandResponse = await options.Url
                .AppendPathSegment("api/command")
                .WithHeader("x-functions-key", options.AuthCode)
                .PostJsonAsync(command)
                .ConfigureAwait(false);

            var commandResult = await commandResponse.Content
                .ReadAsAsync<ICommandResult>()
                .ConfigureAwait(false);

            SetResultLinks(commandResult, command.ProjectId);

            return commandResult;
        }

        public async Task<ProjectType> AddAsync(ProjectType projectType)
        {
            var response = await options.Url
                .AppendPathSegment("api/data/projectTypes")
                .WithHeader("x-functions-key", options.AuthCode)
                .PostJsonAsync(projectType)
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<ProjectType>()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<ProjectType> UpdateAsync(ProjectType projectType)
        {
            var response = await options.Url
                .AppendPathSegment("api/data/projectTypes")
                .WithHeader("x-functions-key", options.AuthCode)
                .PutJsonAsync(projectType)
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<ProjectType>()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<ProjectType> DeleteAsync(string projectTypeId)
        {
            var response = await options.Url
                .AppendPathSegments($"api/data/projectTypes/{projectTypeId}")
                .WithHeader("x-functions-key", options.AuthCode)
                .DeleteAsync()
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<ProjectType>()
                .ConfigureAwait(false);

            return result;
        }
    }
}
