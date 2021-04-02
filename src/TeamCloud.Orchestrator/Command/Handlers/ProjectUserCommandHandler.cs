/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
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
    public sealed class ProjectUserCommandHandler : CommandHandler,
          ICommandHandler<ProjectUserCreateCommand>,
          ICommandHandler<ProjectUserUpdateCommand>,
          ICommandHandler<ProjectUserDeleteCommand>
    {
        private readonly IUserRepository userRepository;
        private readonly IComponentRepository componentRepository;

        public ProjectUserCommandHandler(IUserRepository userRepository, IComponentRepository componentRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        }

        public async Task<ICommandResult> HandleAsync(ProjectUserCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await userRepository
                    .SetAsync(command.Payload)
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }
            finally
            {
                await UpdateComponentsAsync(command.User, command.ProjectId, commandQueue)
                    .ConfigureAwait(false);
            }

            return commandResult;
        }

        public async Task<ICommandResult> HandleAsync(ProjectUserUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await userRepository
                    .SetAsync(command.Payload)
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }
            finally
            {
                await UpdateComponentsAsync(command.User, command.ProjectId, commandQueue)
                    .ConfigureAwait(false);
            }

            return commandResult;
        }

        public async Task<ICommandResult> HandleAsync(ProjectUserDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await userRepository
                    .RemoveAsync(command.Payload)
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }
            finally
            {
                await UpdateComponentsAsync(command.User, command.ProjectId, commandQueue)
                    .ConfigureAwait(false);
            }

            return commandResult;
        }

        private async Task UpdateComponentsAsync(User user, string projectId, IAsyncCollector<ICommand> commandQueue)
        {
            await foreach (var component in componentRepository.ListAsync(projectId))
            {
                var command = new ComponentUpdateCommand(user, component);

                await commandQueue
                    .AddAsync(command)
                    .ConfigureAwait(false);
            }
        }
    }
}
