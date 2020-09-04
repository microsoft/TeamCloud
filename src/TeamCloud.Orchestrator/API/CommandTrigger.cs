/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamCloud.Http;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Validation;
using TeamCloud.Orchestration.Auditing;
using TeamCloud.Orchestrator.Handlers;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator
{
    public class CommandTrigger
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICommandAuditWriter commandAuditWriter;

        public CommandTrigger(IHttpContextAccessor httpContextAccessor, ICommandAuditWriter commandAuditWriter)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.commandAuditWriter = commandAuditWriter ?? throw new ArgumentNullException(nameof(commandAuditWriter));
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

                if (command is null)
                    return new BadRequestResult();

                command.Validate(throwOnValidationError: true);
            }
            catch (ValidationException)
            {
                return new BadRequestResult();
            }

            if (TryGetOrchestratorCommandHandler(command, out var commandHandler))
            {
                ICommandResult commandResult = null;

                try
                {
                    await commandAuditWriter
                        .AuditAsync(command)
                        .ConfigureAwait(false);

                    commandResult = await commandHandler
                        .HandleAsync(command, durableClient)
                        .ConfigureAwait(false);

                    commandResult ??= await durableClient
                        .GetCommandResultAsync(command)
                        .ConfigureAwait(false);

                    if (commandResult is null)
                        throw new NullReferenceException($"Unable to resolve result information for command {command.CommandId}");
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);

                    // there are some edge cases that affect our action result
                    // by returning specific result objects / status codes:

                    switch (exc)
                    {
                        case NotImplementedException notImplementedException:

                            // indicator something in the command's payload can't be processed 

                            return new BadRequestResult();

                        case NotSupportedException notSupportedException:

                            // indicator for a duplicate command 

                            return new System.Web.Http.ConflictResult();
                    }
                }
                finally
                {
                    await commandAuditWriter
                        .AuditAsync(command, commandResult)
                        .ConfigureAwait(false);
                }

                return CreateCommandResultResponse(commandResult);
            }
            else
            {
                return new BadRequestResult();
            }

            bool TryGetOrchestratorCommandHandler(IOrchestratorCommand orchestratorCommand, out IOrchestratorCommandHandler orchestratorCommandHandler)
            {
                var scope = httpContextAccessor.HttpContext.RequestServices
                    .CreateScope();

                orchestratorCommandHandler = scope.ServiceProvider
                    .GetServices<IOrchestratorCommandHandler>()
                    .SingleOrDefault(handler => handler.CanHandle(orchestratorCommand));

                return !(orchestratorCommandHandler is null);
            }
        }

        private async Task<IActionResult> HandleGetAsync(IDurableClient durableClient, Guid commandId)
        {
            var commandResult = await durableClient
                .GetCommandResultAsync(commandId)
                .ConfigureAwait(false);

            if (commandResult is null)
                return new NotFoundResult();

            return CreateCommandResultResponse(commandResult);
        }

        private IActionResult CreateCommandResultResponse(ICommandResult commandResult)
        {
            if (commandResult.RuntimeStatus.IsFinal())
                return new OkObjectResult(commandResult);

            var location = UriHelper.GetDisplayUrl(httpContextAccessor.HttpContext.Request);

            if (!location.EndsWith(commandResult.CommandId.ToString()))
                location = location.AppendPathSegment(commandResult.CommandId);

            return new AcceptedResult(location, commandResult);
        }
    }
}
