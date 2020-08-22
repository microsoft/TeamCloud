/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Handlers
{
    public sealed class OrchestratorProviderDataCommandHandler
        : IOrchestratorCommandHandler<OrchestratorProviderDataCreateCommand>,
          IOrchestratorCommandHandler<OrchestratorProviderDataUpdateCommand>,
          IOrchestratorCommandHandler<OrchestratorProviderDataDeleteCommand>
    {
        private readonly IProviderDataRepository providerDataRepository;

        public OrchestratorProviderDataCommandHandler(IProviderDataRepository providerDataRepository)
        {
            this.providerDataRepository = providerDataRepository ?? throw new ArgumentNullException(nameof(providerDataRepository));
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorProviderDataCreateCommand orchestratorCommand)
        {
            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await providerDataRepository
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

        public async Task<ICommandResult> HandleAsync(OrchestratorProviderDataUpdateCommand orchestratorCommand)
        {
            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await providerDataRepository
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

        public async Task<ICommandResult> HandleAsync(OrchestratorProviderDataDeleteCommand orchestratorCommand)
        {
            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await providerDataRepository
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
