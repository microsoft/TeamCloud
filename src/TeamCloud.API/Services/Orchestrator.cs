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
using TeamCloud.Model.Commands.Core;
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

            if (projectId.HasValue)
            {
                commandResult.Links.Add("status", new Uri(baseUrl, $"api/projects/{projectId}/status/{commandResult.CommandId}").ToString());
            }
            else
            {
                commandResult.Links.Add("status", new Uri(baseUrl, $"api/status/{commandResult.CommandId}").ToString());
            }

            if (commandResult is OrchestratorTeamCloudCreateCommandResult)
            {
                commandResult.Links.Add("location", new Uri(baseUrl, "api/config").ToString());
            }
            else if (commandResult is ICommandResult<User> commandResultUserWithProject && projectId.HasValue)
            {
                commandResult.Links.Add("location", new Uri(baseUrl, $"api/projects/{projectId}/users/{commandResultUserWithProject.Result?.Id}").ToString());
            }
            else if (commandResult is ICommandResult<User> commandResultUserWithoutProject)
            {
                commandResult.Links.Add("location", new Uri(baseUrl, $"api/users/{commandResultUserWithoutProject.Result?.Id}").ToString());
            }
            else if (projectId.HasValue)
            {
                commandResult.Links.Add("location", new Uri(baseUrl, "api/projects/{projectId}").ToString());
            }
        }

        public async Task<ICommandResult> QueryAsync(Guid commandId, Guid? projectId)
        {
            var resultJson = await options.Url
                .AppendPathSegment($"api/command/{commandId}")
                .WithHeader("x-functions-key", options.AuthCode)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .GetStringAsync()
                .ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<ICommandResult>(resultJson);

            if (result != null)
                SetResultLinks(result, projectId);

            return result;
        }

        public async Task<ICommandResult> InvokeAsync(IOrchestratorCommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            try
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
            catch (FlurlHttpException ex) when ((ex.Call.HttpStatus ?? ex.Call.Response.StatusCode) == HttpStatusCode.ServiceUnavailable)
            {
                var unavailbleResult = command.CreateResult();
                unavailbleResult.Errors.Add(ex);
                return unavailbleResult;
            }
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
