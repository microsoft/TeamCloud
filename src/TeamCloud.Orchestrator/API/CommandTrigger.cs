/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation;
using Flurl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using TeamCloud.Http;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Validation;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator
{
    public class CommandTrigger
    {
        internal static string GetCommandOrchestrationInstanceId(Guid commandId)
            => commandId.ToString();

        internal static string GetCommandOrchestrationInstanceId(ICommand command)
            => GetCommandOrchestrationInstanceId(command.CommandId);

        internal static string GetCommandWrapperOrchestrationInstanceId(Guid commandId)
            => $"{GetCommandOrchestrationInstanceId(commandId)}-wrapper";

        internal static string GetCommandWrapperOrchestrationInstanceId(ICommand command)
            => GetCommandWrapperOrchestrationInstanceId(command.CommandId);

        private readonly IHttpContextAccessor httpContextAccessor;

        public CommandTrigger(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        [FunctionName(nameof(CommandTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", "get", Route = "command/{commandId:guid?}")] HttpRequestMessage requestMessage,
            [DurableClient] IDurableClient durableClient,
            string commandId,
            ILogger log)
        {
            if (requestMessage is null)
                throw new ArgumentNullException(nameof(requestMessage));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            IActionResult actionResult;

            try
            {
                switch (requestMessage)
                {
                    case HttpRequestMessage msg when msg.Method == HttpMethod.Get:

                        if (string.IsNullOrEmpty(commandId))
                            actionResult = new NotFoundResult();
                        else
                            actionResult = await HandleGetAsync(durableClient, Guid.Parse(commandId)).ConfigureAwait(false);

                        break;

                    case HttpRequestMessage msg when msg.Method == HttpMethod.Post:

                        actionResult = await HandlePostAsync(durableClient, requestMessage, log).ConfigureAwait(false);

                        break;

                    default:
                        throw new NotSupportedException($"Http method '{requestMessage.Method}' is not supported");
                };
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Processing request failed: {requestMessage.Method.ToString().ToUpperInvariant()} {requestMessage.RequestUri}");

                throw; // re-throw exception and use the default InternalServerError behaviour
            }

            return actionResult;
        }

        private async Task<IActionResult> HandlePostAsync(IDurableClient durableClient, HttpRequestMessage requestMessage, ILogger log)
        {
            IOrchestratorCommand command;

            try
            {
                command = await requestMessage.Content
                    .ReadAsJsonAsync<IOrchestratorCommand>()
                    .ConfigureAwait(false);

                command?.Validate(throwOnValidationError: true);
            }
            catch (ValidationException)
            {
                return new BadRequestResult();
            }

            if (command is null)
                return new BadRequestResult();

            var wapperInstanceId = GetCommandWrapperOrchestrationInstanceId(command.CommandId);

            try
            {
                _ = await durableClient
                    .StartNewAsync(nameof(OrchestratorCommandOrchestration), wapperInstanceId, command)
                    .ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                // check if there is an orchestration for the given
                // orchstrator command message is already in-flight

                var commandWarpperStatus = await durableClient
                    .GetStatusAsync(wapperInstanceId)
                    .ConfigureAwait(false);

                if (commandWarpperStatus is null)
                    throw; // bubble exception

                return new System.Web.Http.ConflictResult();
            }

            return await HandleGetAsync(durableClient, command.CommandId)
                .ConfigureAwait(false);
        }

        private async Task<IActionResult> HandleGetAsync(IDurableClient durableClient, Guid commandId)
        {
            var wrapperInstanceId = GetCommandWrapperOrchestrationInstanceId(commandId);

            var wrapperInstanceStatus = await durableClient
                .GetStatusAsync(wrapperInstanceId)
                .ConfigureAwait(false);

            var command = wrapperInstanceStatus?.Input
                .ToObject<IOrchestratorCommand>();

            if (command is null)
                return new NotFoundResult();

            var commandResult = await durableClient
                .GetCommandResultAsync(command)
                .ConfigureAwait(false);

            if (commandResult is null)
            {
                commandResult = command.CreateResult(wrapperInstanceStatus);
            }

            if (commandResult.RuntimeStatus.IsFinal())
                return new OkObjectResult(commandResult);

            var location = UriHelper.GetDisplayUrl(httpContextAccessor.HttpContext.Request)
                .AppendPathSegment(commandResult.CommandId);

            return new AcceptedResult(location, commandResult);
        }
    }
}
