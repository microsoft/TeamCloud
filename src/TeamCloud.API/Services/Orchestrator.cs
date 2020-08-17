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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services
{
    public class Orchestrator
    {
        private readonly IOrchestratorOptions options;
        private readonly IHttpContextAccessor httpContextAccessor;

        public Orchestrator(IOrchestratorOptions options, IHttpContextAccessor httpContextAccessor)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private void SetResultLinks(ICommandResult commandResult, string projectId)
        {
            var baseUrl = httpContextAccessor.HttpContext.GetApplicationBaseUrl();

            if (string.IsNullOrEmpty(projectId))
            {
                commandResult.Links.Add("status", new Uri(baseUrl, $"api/status/{commandResult.CommandId}").ToString());
            }
            else
            {
                commandResult.Links.Add("status", new Uri(baseUrl, $"api/projects/{projectId}/status/{commandResult.CommandId}").ToString());
            }

            if (IsDeleteCommandResult(commandResult))
            {
                return;
            }
            else if (commandResult is ICommandResult<UserDocument> userCommandResult)
            {
                if (userCommandResult.Result?.Id is null)
                    return;

                if (string.IsNullOrEmpty(projectId))
                {
                    commandResult.Links.Add("location", new Uri(baseUrl, $"api/users/{userCommandResult.Result.Id}").ToString());
                }
                else
                {
                    commandResult.Links.Add("location", new Uri(baseUrl, $"api/projects/{projectId}/users/{userCommandResult.Result.Id}").ToString());
                }
            }
            else if (commandResult is ICommandResult<ProviderDocument> providerCommandResult)
            {
                if (providerCommandResult.Result?.Id is null)
                    return;

                commandResult.Links.Add("location", new Uri(baseUrl, $"api/providers/{providerCommandResult.Result.Id}").ToString());
            }
            else if (commandResult is ICommandResult<ProjectDocument> projectCommandResult)
            {
                if (projectCommandResult.Result?.Id is null)
                    return;

                commandResult.Links.Add("location", new Uri(baseUrl, $"api/projects/{projectCommandResult.Result.Id}").ToString());
            }

            static bool IsDeleteCommandResult(ICommandResult result)
                => result is OrchestratorProjectDeleteCommandResult
                || result is OrchestratorProjectUserDeleteCommandResult
                || result is OrchestratorProviderDeleteCommandResult
                || result is OrchestratorTeamCloudUserDeleteCommandResult;
        }

        public async Task<ICommandResult> QueryAsync(Guid commandId, string projectId)
        {
            var resultJson = await options.Url
                .AppendPathSegment($"api/command/{commandId}")
                .WithHeader("x-functions-key", options.AuthCode)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .GetStringAsync()
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(resultJson))
                return null;

            var result = JsonConvert.DeserializeObject<ICommandResult>(resultJson);

            if (result != null)
                SetResultLinks(result, projectId);

            return result;
        }

        public async Task<ICommandResult> InvokeAsync(IOrchestratorCommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();

            try
            {
                var commandResponse = await options.Url
                    .AppendPathSegment("api/command")
                    .WithHeader("x-functions-key", options.AuthCode)
                    .PostJsonAsync(command)
                    .ConfigureAwait(false);

                commandResult = await commandResponse.Content
                    .ReadAsAsync<ICommandResult>()
                    .ConfigureAwait(false);

                SetResultLinks(commandResult, command.ProjectId);
            }
            catch (FlurlHttpTimeoutException timeoutExc)
            {
                commandResult ??= command.CreateResult();
                commandResult.Errors.Add(timeoutExc);
            }
            catch (FlurlHttpException serviceUnavailableExc) when (serviceUnavailableExc.Call.HttpStatus == HttpStatusCode.ServiceUnavailable)
            {
                commandResult ??= command.CreateResult();
                commandResult.Errors.Add(serviceUnavailableExc);
            }

            return commandResult;
        }

        public async Task<TeamCloudInstanceDocument> SetAsync(TeamCloudInstanceDocument teamCloudInstance)
        {
            var response = await options.Url
                .AppendPathSegment("api/data/teamCloudInstance")
                .WithHeader("x-functions-key", options.AuthCode)
                .PostJsonAsync(teamCloudInstance)
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<TeamCloudInstanceDocument>()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<ProjectTypeDocument> AddAsync(ProjectTypeDocument projectType)
        {
            var response = await options.Url
                .AppendPathSegment("api/data/projectTypes")
                .WithHeader("x-functions-key", options.AuthCode)
                .PostJsonAsync(projectType)
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<ProjectTypeDocument>()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<ProjectTypeDocument> UpdateAsync(ProjectTypeDocument projectType)
        {
            var response = await options.Url
                .AppendPathSegment("api/data/projectTypes")
                .WithHeader("x-functions-key", options.AuthCode)
                .PutJsonAsync(projectType)
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<ProjectTypeDocument>()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<ProjectTypeDocument> DeleteAsync(string projectTypeId)
        {
            var response = await options.Url
                .AppendPathSegments($"api/data/projectTypes/{projectTypeId}")
                .WithHeader("x-functions-key", options.AuthCode)
                .DeleteAsync()
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<ProjectTypeDocument>()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<ProviderDataDocument> AddAsync(ProviderDataDocument providerData)
        {
            var response = await options.Url
                .AppendPathSegment("api/data/providerData")
                .WithHeader("x-functions-key", options.AuthCode)
                .PostJsonAsync(providerData)
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<ProviderDataDocument>()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<ProviderDataDocument> UpdateAsync(ProviderDataDocument providerData)
        {
            var response = await options.Url
                .AppendPathSegment("api/data/providerData")
                .WithHeader("x-functions-key", options.AuthCode)
                .PutJsonAsync(providerData)
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<ProviderDataDocument>()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<ProviderDataDocument> DeleteAsync(ProviderDataDocument providerData)
        {
            if (providerData is null)
                throw new ArgumentNullException(nameof(providerData));

            var response = await options.Url
                .AppendPathSegments($"api/data/providerData/{providerData.Id}")
                .WithHeader("x-functions-key", options.AuthCode)
                .DeleteAsync()
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadAsAsync<ProviderDataDocument>()
                .ConfigureAwait(false);

            return result;
        }
    }
}
