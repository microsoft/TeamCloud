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
    public sealed class DeploymentScopeCommandHandler
        : ICommandHandler<DeploymentScopeCreateCommand>,
          ICommandHandler<DeploymentScopeUpdateCommand>,
          ICommandHandler<DeploymentScopeDeleteCommand>
    {
        private readonly IDeploymentScopeRepository deploymentScopeRepository;

        public DeploymentScopeCommandHandler(IDeploymentScopeRepository deploymentScopeRepository)
        {
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        }

        public async Task<ICommandResult> HandleAsync(DeploymentScopeCreateCommand command, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await deploymentScopeRepository
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

        public async Task<ICommandResult> HandleAsync(DeploymentScopeUpdateCommand command, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await deploymentScopeRepository
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

        public async Task<ICommandResult> HandleAsync(DeploymentScopeDeleteCommand command, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await deploymentScopeRepository
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
    }
}
