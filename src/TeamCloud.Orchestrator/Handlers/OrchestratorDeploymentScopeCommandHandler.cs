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
    public sealed class OrchestratorDeploymentScopeCommandHandler
        : IOrchestratorCommandHandler<OrchestratorDeploymentScopeCreateCommand>,
          IOrchestratorCommandHandler<OrchestratorDeploymentScopeUpdateCommand>,
          IOrchestratorCommandHandler<OrchestratorDeploymentScopeDeleteCommand>
    {
        private readonly IDeploymentScopeRepository deploymentScopeRepository;

        public OrchestratorDeploymentScopeCommandHandler(IDeploymentScopeRepository deploymentScopeRepository)
        {
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorDeploymentScopeCreateCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await deploymentScopeRepository
                    .AddAsync(orchestratorCommand.Payload)
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorDeploymentScopeUpdateCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await deploymentScopeRepository
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

        public async Task<ICommandResult> HandleAsync(OrchestratorDeploymentScopeDeleteCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await deploymentScopeRepository
                    .RemoveAsync(orchestratorCommand.Payload)
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
