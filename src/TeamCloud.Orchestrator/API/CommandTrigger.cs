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
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Audit;
using TeamCloud.Http;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Validation;
using TeamCloud.Orchestrator.Command;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.API
{
    public class CommandTrigger
    {
        private readonly ICommandHandler[] commandHandlers;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICommandAuditWriter commandAuditWriter;

        public CommandTrigger(ICommandHandler[] commandHandlers, IHttpContextAccessor httpContextAccessor, ICommandAuditWriter commandAuditWriter)
        {
            this.commandHandlers = commandHandlers ?? throw new ArgumentNullException(nameof(commandHandlers));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.commandAuditWriter = commandAuditWriter ?? throw new ArgumentNullException(nameof(commandAuditWriter));
        }

        [FunctionName(nameof(CommandTrigger) + nameof(Receive))]
        public async Task<IActionResult> Receive(
            [HttpTrigger(AuthorizationLevel.Function, "post", "get", Route = "command/{commandId:guid?}")] HttpRequestMessage requestMessage,
            [Queue(ICommandHandler.ProcessorQueue)] IAsyncCollector<ICommand> commandProcessor,
            [Queue(ICommandHandler.MonitorQueue)] IAsyncCollector<string> commandMonitor,
            [DurableClient] IDurableClient durableClient,
            string commandId,
            ILogger log)
        {
            if (requestMessage is null)
                throw new ArgumentNullException(nameof(requestMessage));

            if (commandProcessor is null)
                throw new ArgumentNullException(nameof(commandProcessor));

            if (commandMonitor is null)
                throw new ArgumentNullException(nameof(commandMonitor));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            try
            {
                var actionResultTask = requestMessage switch
                {
                    HttpRequestMessage msg when msg.Method == HttpMethod.Get && Guid.TryParse(commandId, out var commandIdParsed)
                        => ResolveCommandResultAsync(durableClient, commandIdParsed),

                    HttpRequestMessage msg when msg.Method == HttpMethod.Post
                        => ProcessCommandAsync(durableClient, requestMessage, commandProcessor, commandMonitor, log),

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

        [FunctionName(nameof(CommandTrigger) + nameof(Dequeue))]
        public async Task Dequeue(
            [QueueTrigger(ICommandHandler.ProcessorQueue)] CloudQueueMessage commandMessage,
            [Queue(ICommandHandler.ProcessorQueue)] IAsyncCollector<ICommand> commandQueue,
            [Queue(ICommandHandler.MonitorQueue)] IAsyncCollector<string> commandMonitor,
            [DurableClient] IDurableClient durableClient,
            ILogger log)
        {
            if (commandMessage is null)
                throw new ArgumentNullException(nameof(commandMessage));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            if (commandMonitor is null)
                throw new ArgumentNullException(nameof(commandMonitor));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            try
            {
                var command = TeamCloudSerialize.DeserializeObject<ICommand>(commandMessage.AsString);

                command.Validate(throwOnValidationError: true);

                _ = await ProcessCommandAsync(durableClient, command, commandQueue, commandMonitor, log)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Failed to process queued command: {exc.Message}");

                throw;
            }
        }

        [FunctionName(nameof(CommandTrigger) + nameof(Monitor))]
        public async Task Monitor(
            [QueueTrigger(ICommandHandler.MonitorQueue)] CloudQueueMessage commandMessage,
            [Queue(ICommandHandler.ProcessorQueue)] IAsyncCollector<ICommand> commandQueue,
            [Queue(ICommandHandler.MonitorQueue)] CloudQueue commandMonitor,
            [DurableClient] IDurableClient durableClient,
            ILogger log)
        {
            if (commandMessage is null)
                throw new ArgumentNullException(nameof(commandMessage));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            if (commandMonitor is null)
                throw new ArgumentNullException(nameof(commandMonitor));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (Guid.TryParse(commandMessage.AsString, out var commandId))
            {
                try
                {
                    var command = await durableClient
                        .GetCommandAsync(commandId)
                        .ConfigureAwait(false);

                    if (command is null)
                    {
                        // we could find a command based on the enqueued command id - warn and forget

                        log.LogWarning($"Monitoring command failed: Could not find command {commandId}");
                    }
                    else
                    {
                        var commandResult = await durableClient
                            .GetCommandResultAsync(commandId)
                            .ConfigureAwait(false);

                        await commandAuditWriter
                            .AuditAsync(command, commandResult)
                            .ConfigureAwait(false);

                        if (commandResult?.RuntimeStatus.IsFinal() ?? false)
                        {
                            var commandCollector = new CommandCollector(commandQueue, command);

                            await BroadcastCommandResultAsync(commandCollector, command, commandResult, log)
                                .ConfigureAwait(false);
                        }
                        else
                        {
                            // the command result is still not in a final state - as we want to monitor the command until it is done,
                            // we are going to re-enqueue the command ID with a visibility offset to delay the next result lookup.

                            await commandMonitor
                                .AddMessageAsync(new CloudQueueMessage(commandId.ToString()), null, TimeSpan.FromSeconds(3), null, null)
                                .ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception exc)
                {
                    log.LogError(exc, $"Monitoring command failed: {exc.Message}");

                    throw;
                }
            }
            else
            {
                // we expect that the queue message is a valid guid (command ID) - warn and forget

                log.LogWarning($"Monitoring command failed: Invalid command ID ({commandMessage.AsString})");
            }
        }

        private async Task<IActionResult> ProcessCommandAsync(IDurableClient durableClient, HttpRequestMessage commandMessage, IAsyncCollector<ICommand> commandQueue, IAsyncCollector<string> commandMonitor, ILogger log)
        {
            ICommand command = null;

            try
            {
                command = await commandMessage.Content
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

            return await ProcessCommandAsync(durableClient, command, commandQueue, commandMonitor, log)
                .ConfigureAwait(false);
        }

        private async Task<IActionResult> ProcessCommandAsync(IDurableClient durableClient, ICommand command, IAsyncCollector<ICommand> commandQueue, IAsyncCollector<string> commandMonitor, ILogger log)
        {
            var commandHandler = commandHandlers.SingleOrDefault(handler => handler.CanHandle(command));
            var commandCollector = new CommandCollector(commandQueue, command);

            if (commandHandler is null)
            {
                return new BadRequestResult();
            }
            else
            {
                ICommandResult commandResult = null;

                try
                {
                    await commandAuditWriter
                        .AuditAsync(command)
                        .ConfigureAwait(false);

                    if (commandHandler.Orchestration)
                    {
                        _ = await durableClient
                            .StartNewAsync(nameof(CommandOrchestration), command.CommandId.ToString(), command)
                            .ConfigureAwait(false);

                        commandResult = await durableClient
                            .GetCommandResultAsync(command)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        commandResult = await commandHandler
                            .HandleAsync(command, commandCollector, durableClient, null, log ?? NullLogger.Instance)
                            .ConfigureAwait(false);

                        if (!commandResult.RuntimeStatus.IsFinal())
                            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
                    }

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

                            // indicates something in the command's payload can't be processed

                            return new BadRequestResult();

                        case NotSupportedException notSupportedException:

                            // indicates a duplicate command (identified by id)

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

                    if (!commandHandler.Orchestration)
                    {
                        // only broadcast command results if the command handler doesn't
                        // require an orchestration - in case the command handler is orchstration
                        // based, its up to the command monitor to broadcast the command result
                        // when command processing has finished.

                        await BroadcastCommandResultAsync(commandCollector, command, commandResult, log)
                            .ConfigureAwait(false);
                    }
                }

                return CreateCommandResultResponse(commandResult);
            }
        }

        private static async Task BroadcastCommandResultAsync(IAsyncCollector<ICommand> commandCollector, ICommand command, ICommandResult commandResult, ILogger log)
        {
            if (command.CommandAction != CommandAction.Custom)
            {
                // command result broadcasting is only available for command actions of type: create, update, and delete

                try
                {
                    var broadcastCommandType = typeof(BroadcastCommandResultCommand<>).MakeGenericType(commandResult.GetType());
                    var broadcastCommand = (ICommand)Activator.CreateInstance(broadcastCommandType, command.User, commandResult);

                    await commandCollector
                        .AddAsync(broadcastCommand)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    log.LogError(exc, $"Failed to enqueue command result broadcast for '{command.CommandId}': {exc.Message}");
                }
            }
        }

        private async Task<IActionResult> ResolveCommandResultAsync(IDurableClient durableClient, Guid commandId)
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

            var location = httpContextAccessor.HttpContext?.Request?.GetDisplayUrl();

            if (string.IsNullOrEmpty(location))
                return new AcceptedResult();

            if (!location.EndsWith(commandResult.CommandId.ToString(), StringComparison.OrdinalIgnoreCase))
                location = location.AppendPathSegment(commandResult.CommandId);

            return new AcceptedResult(location, commandResult);
        }

    }
}
