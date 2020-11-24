/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Handlers
{
    public sealed class OrganizationCommandHandler
        : ICommandHandler<OrganizationCreateCommand>,
          ICommandHandler<OrganizationUpdateCommand>,
          ICommandHandler<OrganizationDeleteCommand>
    {
        private readonly IUserRepository userRepository;
        private readonly IOrganizationRepository organizationRepository;

        public OrganizationCommandHandler(IOrganizationRepository organizationRepository, IUserRepository userRepository)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<ICommandResult> HandleAsync(OrganizationCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await organizationRepository
                    .SetAsync(command.Payload)
                    .ConfigureAwait(false);

                await userRepository
                    .AddAsync(command.User)
                    .ConfigureAwait(false);

                await commandQueue
                    .AddAsync(new OrganizationDeployCommand(command.User, commandResult.Result))
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        public async Task<ICommandResult> HandleAsync(OrganizationUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await organizationRepository
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

        public Task<ICommandResult> HandleAsync(OrganizationDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            throw new NotSupportedException();

            // var commandResult = orchestratorCommand.CreateResult();

            // try
            // {
            //     commandResult.Result = await organizationRepository
            //         .RemoveAsync(orchestratorCommand.Payload)
            //         .ConfigureAwait(false);

            //     commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            // }
            // catch (Exception exc)
            // {
            //     commandResult.Errors.Add(exc);
            // }

            // return commandResult;
        }
    }
}
