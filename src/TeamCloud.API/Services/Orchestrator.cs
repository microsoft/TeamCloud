/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
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

        private void SetResultLinks(HttpResponseMessage commandResponse, ICommandResult commandResult, string projectId)
        {
            var baseUrl = httpContextAccessor.HttpContext?.GetApplicationBaseUrl(true);

            if (baseUrl is null)
            {
                return; // as we couldn't resolve a base url, we can't generate status or location urls for our response object
            }

            if (commandResponse.StatusCode == HttpStatusCode.Accepted)
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    commandResult.Links.Add("status", new Uri(baseUrl, $"api/status/{commandResult.CommandId}").ToString());
                }
                else
                {
                    commandResult.Links.Add("status", new Uri(baseUrl, $"api/projects/{projectId}/status/{commandResult.CommandId}").ToString());
                }
            }

            if (IsDeleteCommandResult(commandResult))
            {
                return; // delete command don't provide a status location endpoint
            }
            else if (commandResult is ICommandResult<UserDocument> userCommandResult)
            {
                if (string.IsNullOrEmpty(userCommandResult.Result?.Id))
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
            else if (commandResult is ICommandResult<ProviderDocument> providerDocumentResult)
            {
                if (string.IsNullOrEmpty(providerDocumentResult.Result?.Id))
                    return;

                commandResult.Links.Add("location", new Uri(baseUrl, $"api/providers/{providerDocumentResult.Result.Id}").ToString());
            }
            else if (commandResult is ICommandResult<ProviderDataDocument> providerDataDocumentResult)
            {
                if (string.IsNullOrEmpty(providerDataDocumentResult.Result?.Id))
                    return;

                var providerId = providerDataDocumentResult.Result?.ProviderId;

                if (string.IsNullOrEmpty(projectId))
                {
                    commandResult.Links.Add("location", new Uri(baseUrl, $"api/projects/{projectId}/providers/{providerId}/data/{providerDataDocumentResult.Result.Id}").ToString());
                }
                else
                {
                    commandResult.Links.Add("location", new Uri(baseUrl, $"api/providers/{providerId}/data/{providerDataDocumentResult.Result.Id}").ToString());
                }
            }
            else if (commandResult is ICommandResult<ProjectDocument> projectDocumentResult)
            {
                if (string.IsNullOrEmpty(projectDocumentResult.Result?.Id))
                    return;

                commandResult.Links.Add("location", new Uri(baseUrl, $"api/projects/{projectDocumentResult.Result.Id}").ToString());
            }
            else if (commandResult is ICommandResult<ProjectTypeDocument> projectTypeDocumentResult)
            {
                if (string.IsNullOrEmpty(projectTypeDocumentResult.Result?.Id))
                    return;

                commandResult.Links.Add("location", new Uri(baseUrl, $"api/projectTypes/{projectTypeDocumentResult.Result.Id}").ToString());
            }
            else if (commandResult is ICommandResult<ProjectLinkDocument> projectLinkDocumentResult)
            {
                if (string.IsNullOrEmpty(projectLinkDocumentResult.Result?.Id))
                    return;

                commandResult.Links.Add("location", new Uri(baseUrl, $"api/projects/{projectId}/links/{projectLinkDocumentResult.Result.Id}").ToString());
            }
            else if (commandResult is ICommandResult<TeamCloudInstanceDocument>)
            {
                commandResult.Links.Add("location", new Uri(baseUrl, "api/admin/teamCloudInstance").ToString());
            }


            static bool IsDeleteCommandResult(ICommandResult result)
                => result is OrchestratorProjectDeleteCommandResult
                || result is OrchestratorProjectUserDeleteCommandResult
                || result is OrchestratorProjectLinkDeleteCommandResult
                || result is OrchestratorProviderDeleteCommandResult
                || result is OrchestratorTeamCloudUserDeleteCommandResult;
        }

        public Task<ICommandResult> QueryAsync(ICommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return QueryAsync(command.CommandId, command.ProjectId);
        }

        public async Task<ICommandResult> QueryAsync(Guid commandId, string projectId)
        {
            var commandResponse = await options.Url
                .AppendPathSegment($"api/command/{commandId}")
                .WithHeader("x-functions-key", options.AuthCode)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .GetAsync()
                .ConfigureAwait(false);

            if (commandResponse.StatusCode == HttpStatusCode.NotFound)
                return null;

            var commandResult = await commandResponse.Content
                .ReadAsAsync<ICommandResult>()
                .ConfigureAwait(false);

            SetResultLinks(commandResponse, commandResult, projectId);

            return commandResult;
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

                SetResultLinks(commandResponse, commandResult, command.ProjectId);
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
    }
}
