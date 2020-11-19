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
using TeamCloud.Audit;
using TeamCloud.Http;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Validation;
using TeamCloud.Orchestrator.Handlers;
using TeamCloud.Orchestrator.Operations.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.API
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
            [Queue(MonitorTrigger.CommandMonitorQueue)] IAsyncCollector<string> commandMonitor,
            [DurableClient] IDurableClient durableClient,
            string commandId,
            ILogger log)
        {
            if (requestMessage is null)
                throw new ArgumentNullException(nameof(requestMessage));

            if (commandMonitor is null)
                throw new ArgumentNullException(nameof(commandMonitor));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            try
            {
                var actionResultTask = requestMessage switch
                {
                    HttpRequestMessage msg when msg.Method == HttpMethod.Get && Guid.TryParse(commandId, out var commandIdParsed)
                        => HandleGetAsync(durableClient, commandIdParsed),

                    HttpRequestMessage msg when msg.Method == HttpMethod.Post
                        => HandlePostAsync(durableClient, commandMonitor, requestMessage, log),

                    _ => Task.FromResult<IActionResult>(new NotFoundResult())
                };

                return await actionResultTask.ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Processing request failed: {requestMessage.Method.ToString().ToUpperInvariant()} {requestMessage.RequestUri}");

                throw; // re-throw exception and use the default InternalServerError behaviour
            }
        }

        private async Task<IActionResult> HandlePostAsync(IDurableClient durableClient, IAsyncCollector<string> commandMonitor, HttpRequestMessage requestMessage, ILogger log)
        {
            ICommand command = null;

            try
            {
                command = await requestMessage.Content
                    .ReadAsJsonAsync<ICommand>()
                    .ConfigureAwait(false);

                if (command is null)
                    return new BadRequestResult();

                command.Validate(throwOnValidationError: true);
            }
            catch (ValidationException exc)
            {
                log.LogError(exc, $"Command {command?.CommandId} failed validation");

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
                    if (commandResult.RuntimeStatus.IsFinal())
                    {
                        await commandAuditWriter
                            .AuditAsync(command, commandResult)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await commandMonitor
                            .AddAsync(command.CommandId.ToString())
                            .ConfigureAwait(false);
                    }
                }

                return CreateCommandResultResponse(commandResult);
            }
            else
            {
                return new BadRequestResult();
            }

            bool TryGetOrchestratorCommandHandler(ICommand orchestratorCommand, out ICommandHandler orchestratorCommandHandler)
            {
                using var scope = httpContextAccessor.HttpContext.RequestServices.CreateScope();

                var orchestratorCommandHandlers = scope.ServiceProvider
                    .GetServices<ICommandHandler>();

                orchestratorCommandHandler = orchestratorCommandHandlers.SingleOrDefault(handler => handler.CanHandle(orchestratorCommand))
                    ?? orchestratorCommandHandlers.SingleOrDefault(handler => handler.CanHandle(orchestratorCommand, true));

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

            var location = httpContextAccessor.HttpContext.Request.GetDisplayUrl();

            if (!location.EndsWith(commandResult.CommandId.ToString(), StringComparison.OrdinalIgnoreCase))
                location = location.AppendPathSegment(commandResult.CommandId);

            return new AcceptedResult(location, commandResult);
        }
    }
}
