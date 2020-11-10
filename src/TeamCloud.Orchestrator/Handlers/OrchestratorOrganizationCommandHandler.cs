/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Handlers
{
    public sealed class OrchestratorOrganizationCommandHandler
        : IOrchestratorCommandHandler<OrchestratorOrganizationCreateCommand>,
          IOrchestratorCommandHandler<OrchestratorOrganizationUpdateCommand>,
          IOrchestratorCommandHandler<OrchestratorOrganizationDeleteCommand>
    {
        private readonly IUserRepository userRepository;
        private readonly IOrganizationRepository organizationRepository;

        public OrchestratorOrganizationCommandHandler(IOrganizationRepository organizationRepository, IUserRepository userRepository)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorOrganizationCreateCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await organizationRepository
                    .SetAsync(orchestratorCommand.Payload)
                    .ConfigureAwait(false);

                await userRepository
                    .AddAsync(orchestratorCommand.User)
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorOrganizationUpdateCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await organizationRepository
                    .SetAsync(orchestratorCommand.Payload)
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        public Task<ICommandResult> HandleAsync(OrchestratorOrganizationDeleteCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

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
