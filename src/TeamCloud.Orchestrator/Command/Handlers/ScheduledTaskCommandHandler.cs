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

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ScheduledTaskCommandHandler : CommandHandler,
          ICommandHandler<ScheduledTaskCreateCommand>,
          ICommandHandler<ScheduledTaskUpdateCommand>,
          ICommandHandler<ScheduledTaskDeleteCommand>,
          ICommandHandler<ScheduledTaskRunCommand>
    {

        private readonly IScheduledTaskRepository scheduledTaskRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTaskRepository componentTaskRepository;

        public ScheduledTaskCommandHandler(IScheduledTaskRepository scheduledTaskRepository, IComponentRepository componentRepository, IComponentTaskRepository componentTaskRepository)
        {
            this.scheduledTaskRepository = scheduledTaskRepository ?? throw new ArgumentNullException(nameof(scheduledTaskRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        }

        public async Task<ICommandResult> HandleAsync(ScheduledTaskCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await scheduledTaskRepository
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

        public async Task<ICommandResult> HandleAsync(ScheduledTaskUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await scheduledTaskRepository
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

        public async Task<ICommandResult> HandleAsync(ScheduledTaskDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await scheduledTaskRepository
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

        public async Task<ICommandResult> HandleAsync(ScheduledTaskRunCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
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
                    ScheduledTaskId = command.Payload.Id,
                    Type = ComponentTaskType.Custom,
                    TypeName = t.ComponentTaskTemplateId,

                    // component input json is used as a fallback !!!
                    InputJson = t.InputJson ?? ""
                }));

                var commandTasks = commands.Select(c => commandQueue.AddAsync(c));

                await Task.WhenAll(commandTasks)
                    .ConfigureAwait(false);

                command.Payload.LastRun = DateTime.UtcNow;

                commandResult.Result = await scheduledTaskRepository
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
}
