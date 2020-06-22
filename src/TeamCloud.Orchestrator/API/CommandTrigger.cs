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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Http;
using TeamCloud.Model.Internal.Commands;
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

            var instanceId = GetCommandWrapperOrchestrationInstanceId(command.CommandId);

            try
            {
                _ = await durableClient
                    .StartNewAsync(nameof(OrchestratorCommandOrchestration), instanceId, command)
                    .ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                // check if there is an orchestration for the given
                // orchstrator command message is already in-flight

                var commandWarpperStatus = await durableClient
                    .GetStatusAsync(instanceId)
                    .ConfigureAwait(false);

                if (commandWarpperStatus is null)
                    throw; // bubble exception

                return new System.Web.Http.ConflictResult();
            }

            var commandResult = await WaitForCommandResultAsync(durableClient, command, log)
                .ConfigureAwait(false);

            return CreateCommandResultResponse(command, commandResult);
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

            return CreateCommandResultResponse(command, commandResult);
        }

        private IActionResult CreateCommandResultResponse(ICommand command, ICommandResult commandResult)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            if (commandResult.RuntimeStatus.IsFinal())
            {
                // this was damned fast - the orchestration already
                // finished it's work and we can return a final response.

                return new OkObjectResult(commandResult);
            }
            else
            {
                // the usual behavior - the orchestration is in progress
                // so we have to inform the caller that we accepted the command

                var location = UriHelper.GetDisplayUrl(httpContextAccessor.HttpContext.Request)
                    .AppendPathSegment(commandResult.CommandId);

                return new AcceptedResult(location, commandResult);
            }
        }

        private static async Task<ICommandResult> WaitForCommandResultAsync(IDurableClient durableClient, ICommand command, ILogger log)
        {
            var timeoutDuration = TimeSpan.FromMinutes(5);
            var timeout = DateTime.UtcNow.Add(timeoutDuration);

            while (DateTime.UtcNow <= timeout)
            {
                var commandResult = await durableClient
                    .GetCommandResultAsync(command)
                    .ConfigureAwait(false);

                if (commandResult?.RuntimeStatus.IsUnknown() ?? true)
                {
                    log.LogInformation($"Waiting for command orchestration '{command.CommandId}' ...");

                    await Task
                        .Delay(5000) // 5 sec
                        .ConfigureAwait(false);
                }
                else
                {
                    return commandResult;
                }
            }

            throw new TimeoutException($"Failed to get status for command {command.CommandId} within {timeoutDuration}");
        }
    }
}
