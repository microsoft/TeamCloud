/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ComponentCommandHandler
        : ICommandHandler<ComponentCreateCommand>,
          ICommandHandler<ComponentTaskCommand>,
          ICommandHandler<ComponentDeleteCommand>
    {
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTaskRepository componentTaskRepository;

        public ComponentCommandHandler(IComponentRepository componentRepository, IComponentTaskRepository componentTaskRepository)
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        }

        public bool Orchestration => false;

        public async Task<ICommandResult> HandleAsync(ComponentCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await componentRepository
                    .AddAsync(command.Payload)
                    .ConfigureAwait(false);

                if (commandResult.Result.Type == ComponentType.Environment)
                {
                    var componentTask = new ComponentTask
                    {
                        Organization = commandResult.Result.Organization,
                        ComponentId = commandResult.Result.Id,
                        ProjectId = commandResult.Result.ProjectId,
                        Type = ComponentTaskType.Create,
                        RequestedBy = commandResult.Result.Creator,
                        InputJson = commandResult.Result.InputJson
                    };

                    componentTask = await componentTaskRepository
                        .AddAsync(componentTask)
                        .ConfigureAwait(false);

                    await commandQueue
                        .AddAsync(new ComponentTaskRunCommand(command.User, componentTask))
                        .ConfigureAwait(false);
                }

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        public async Task<ICommandResult> HandleAsync(ComponentTaskCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                // var component = await componentRepository
                //     .GetAsync(command.Payload.ProjectId, command.Payload.ComponentId)
                //     .ConfigureAwait(false);

                // if (command.Payload.Type != ComponentType.Environment)
                // {
                //     // ensure the component is of type Environment; otherwise this command is not supported
                //     throw new NotSupportedException($"Command of type {command.GetType().Name} is not supported for components of type {command.Payload.Type}.");
                // }

                commandResult.Result = await componentTaskRepository
                    .AddAsync(command.Payload)
                    .ConfigureAwait(false);

                await commandQueue
                    .AddAsync(new ComponentTaskRunCommand(command.User, commandResult.Result))
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        public async Task<ICommandResult> HandleAsync(ComponentDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await componentRepository
                    .RemoveAsync(command.Payload)
                    .ConfigureAwait(false);

                if (commandResult.Result.Type == ComponentType.Environment)
                {
                    var componentTask = new ComponentTask
                    {
                        Organization = commandResult.Result.Organization,
                        ComponentId = commandResult.Result.Id,
                        ProjectId = commandResult.Result.ProjectId,
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
                }

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
