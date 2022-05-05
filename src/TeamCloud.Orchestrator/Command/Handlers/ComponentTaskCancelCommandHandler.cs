/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Command.Entities;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class ComponentTaskCancelCommandHandler : CommandHandler<ComponentTaskCancelCommand>
{
    private readonly IComponentTaskRepository componentTaskRepository;
    private readonly IAzureService azureService;

    public ComponentTaskCancelCommandHandler(IComponentTaskRepository componentTaskRepository, IAzureService azureService)
    {
        this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        this.azureService = azureService ?? throw new ArgumentNullException(nameof(azureService));
    }

    public override bool Orchestration => false;

    public override async Task<ICommandResult> HandleAsync(ComponentTaskCancelCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = (await componentTaskRepository
                .GetAsync(command.Payload.ComponentId, command.Payload.Id)
                .ConfigureAwait(Orchestration)) ?? command.Payload;

            if (commandResult.Result.Type != ComponentTaskType.Custom)
            {
                throw new Exception($"Component tasks of type '{commandResult.Result.TypeName}' cannot be canceled!");
            }
            else if (commandResult.Result.TaskState.IsFinal())
            {
                throw new Exception($"Component tasks in state '{commandResult.Result.TaskState}' cannot be canceled!");
            }
            else
            {
                var status = await orchestrationContext
                    .GetCommandStatusAsync(commandResult.Result, showInput: false)
                    .ConfigureAwait(Orchestration);

                if (status is not null && status.RuntimeStatus.IsActive())
                {
                    await orchestrationContext
                        .TerminateCommandAsync(commandResult.Result, $"Canceled by user {command.User.DisplayName}")
                        .ConfigureAwait(Orchestration);
                }

                if (!string.IsNullOrEmpty(commandResult.Result.ResourceId))
                {
                    await azureService
                        .DeleteResourceAsync(commandResult.Result.ResourceId, deleteLocks: true)
                        .ConfigureAwait(Orchestration);
                }

                commandResult.Result.TaskState = TaskState.Canceled;
                commandResult.Result.Finished = DateTime.UtcNow;
                commandResult.Result.ResourceId = null;

                commandResult.Result = await componentTaskRepository
                    .SetAsync(commandResult.Result)
                    .ConfigureAwait(Orchestration);

            }

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }
        finally
        {
            await orchestrationContext
                .CleanupResourceLocksAsync()
                .ConfigureAwait(Orchestration);
        }

        return commandResult;
    }
}
