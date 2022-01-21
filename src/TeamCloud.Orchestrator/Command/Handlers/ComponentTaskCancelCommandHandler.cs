/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class ComponentTaskCancelCommandHandler : CommandHandler<ComponentTaskCancelCommand>
{
    private readonly IComponentTaskRepository componentTaskRepository;
    private readonly IAzureResourceService azureResourceService;

    public ComponentTaskCancelCommandHandler(IComponentTaskRepository componentTaskRepository, IAzureResourceService azureResourceService)
    {
        this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
    }

    public override bool Orchestration => false;

    public override async Task<ICommandResult> HandleAsync(ComponentTaskCancelCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
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
                var status = await orchestrationClient
                    .GetStatusAsync(commandResult.Result.Id, showInput: false)
                    .ConfigureAwait(Orchestration);

                if (status is not null && status.RuntimeStatus.IsActive())
                {
                    await orchestrationClient
                        .TerminateAsync(commandResult.Result.Id, $"Canceled by user {command.User.DisplayName}")
                        .ConfigureAwait(Orchestration);
                }

                if (AzureResourceIdentifier.TryParse(commandResult.Result.ResourceId, out var resourceId))
                {
                    var containerGroup = await azureResourceService
                        .GetResourceAsync<AzureContainerGroupResource>(resourceId.ToString())
                        .ConfigureAwait(false);

                    if (containerGroup is not null)
                    {
                        await containerGroup
                            .DeleteAsync(true)
                            .ConfigureAwait(false);
                    }
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
            // Get rid of orphan locks to unblock waiting orchestrations

            await orchestrationClient
                .CleanEntityStorageAsync(true, true, CancellationToken.None)
                .ConfigureAwait(false);
        }

        return commandResult;
    }
}
