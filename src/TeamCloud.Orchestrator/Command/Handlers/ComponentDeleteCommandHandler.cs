/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class ComponentDeleteCommandHandler : CommandHandler<ComponentDeleteCommand>
{
    private readonly IComponentRepository componentRepository;
    private readonly IComponentTaskRepository componentTaskRepository;

    public ComponentDeleteCommandHandler(IComponentRepository componentRepository, IComponentTaskRepository componentTaskRepository)
    {
        this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
    }

    public override bool Orchestration => false;

    public override async Task<ICommandResult> HandleAsync(ComponentDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await componentRepository
                .RemoveAsync(command.Payload, true)
                .ConfigureAwait(false);

            var existingComponentTasks = componentTaskRepository
                .ListAsync(commandResult.Result.Id)
                .Where(ct => ct.Type == ComponentTaskType.Custom && ct.TaskState.IsActive());

            await foreach (var existingComponentTask in existingComponentTasks)
            {
                var cancelCommand = new ComponentTaskCancelCommand(command.User, existingComponentTask);

                await commandQueue
                    .AddAsync(cancelCommand)
                    .ConfigureAwait(false);
            }

            var componentTask = new ComponentTask
            {
                Organization = commandResult.Result.Organization,
                OrganizationName = commandResult.Result.OrganizationName,
                ComponentId = commandResult.Result.Id,
                ComponentName = commandResult.Result.Slug,
                ProjectId = commandResult.Result.ProjectId,
                ProjectName = commandResult.Result.ProjectName,
                Type = ComponentTaskType.Delete,
                RequestedBy = command.User.Id,
                InputJson = commandResult.Result.InputJson
            };

            componentTask = await componentTaskRepository
                .AddAsync(componentTask)
                .ConfigureAwait(false);

            await commandQueue
                .AddAsync(new ComponentTaskRunCommand(command.User, componentTask))
                .ConfigureAwait(false);

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }

}
