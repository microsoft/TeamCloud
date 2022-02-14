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
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class ScheduleCommandHandler : CommandHandler,
      ICommandHandler<ScheduleCreateCommand>,
      ICommandHandler<ScheduleUpdateCommand>,
      ICommandHandler<ScheduleDeleteCommand>,
      ICommandHandler<ScheduleRunCommand>
{

    private readonly IScheduleRepository scheduleRepository;

    public ScheduleCommandHandler(IScheduleRepository scheduleRepository)
    {
        this.scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
    }

    public override bool Orchestration => false;

    public async Task<ICommandResult> HandleAsync(ScheduleCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await scheduleRepository
                .AddAsync(command.Payload)
                .ConfigureAwait(false);

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }

    public async Task<ICommandResult> HandleAsync(ScheduleUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await scheduleRepository
                .SetAsync(command.Payload)
                .ConfigureAwait(false);

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }

    public async Task<ICommandResult> HandleAsync(ScheduleDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await scheduleRepository
                .RemoveAsync(command.Payload)
                .ConfigureAwait(false);

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }

    public async Task<ICommandResult> HandleAsync(ScheduleRunCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            var commands = command.Payload.ComponentTasks.Select(t => new ComponentTaskCreateCommand(command.User, new ComponentTask
            {
                Organization = command.Payload.Organization,
                ProjectId = command.Payload.ProjectId,
                ComponentId = t.ComponentId,
                RequestedBy = command.User.Id,
                ScheduleId = command.Payload.Id,
                Type = ComponentTaskType.Custom,
                TypeName = t.ComponentTaskTemplateId,

                // component input json is used as a fallback !!!
                InputJson = t.InputJson ?? ""
            }));

            await commands
                .Select(c => commandQueue.AddAsync(c))
                .WhenAll()
                .ConfigureAwait(false);

            command.Payload.LastRun = DateTime.UtcNow;

            commandResult.Result = await scheduleRepository
                .SetAsync(command.Payload)
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
