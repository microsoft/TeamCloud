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
using TeamCloud.Model;

namespace TeamCloud.API
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
                commandResult.Links.Add("status", new Uri(baseUrl, $"api/projects/{projectId}/status/{commandResult.InstanceId}").ToString());
                commandResult.Links.Add("project", new Uri(baseUrl, $"api/projects/{projectId}").ToString());
            }
            else
            {
                commandResult.Links.Add("status", new Uri(baseUrl, $"api/status/{commandResult.InstanceId}").ToString());
            }
        }

        public async Task<ICommandResult> QueryAsync(Guid correlationId, Guid? projectId)
        {
            var result = await options.Url
                .AppendPathSegment($"api/orchestrator/{correlationId}")
                .WithHeader("x-functions-key", options.AuthCode)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .GetJsonAsync<ICommandResult>()
                .ConfigureAwait(false);

            SetResultLinks(result, projectId);

            return result;
        }

        public async Task<ICommandResult<TResult>> InvokeAsync<TResult>(ICommand command)
            where TResult : new()
        {
            var commandResponse = await options.Url
                .AppendPathSegment("/api/orchestrator")
                .WithHeader("x-functions-key", options.AuthCode)
                .PostJsonAsync(command)
                .ConfigureAwait(false);

            var commandResult = await commandResponse
                .GetJsonAsync<ICommandResult<TResult>>()
                .ConfigureAwait(false);

            SetResultLinks(commandResult, command.ProjectId);

            return commandResult;
        }
    }
}
