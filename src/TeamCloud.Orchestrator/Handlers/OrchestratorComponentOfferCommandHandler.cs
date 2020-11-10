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
    public sealed class OrchestratorComponentOfferCommandHandler
        : IOrchestratorCommandHandler<OrchestratorComponentOfferCreateCommand>,
          IOrchestratorCommandHandler<OrchestratorComponentOfferUpdateCommand>,
          IOrchestratorCommandHandler<OrchestratorComponentOfferDeleteCommand>
    {
        private readonly IComponentOfferRepository componentOfferRepository;

        public OrchestratorComponentOfferCommandHandler(IComponentOfferRepository componentOfferRepository)
        {
            this.componentOfferRepository = componentOfferRepository ?? throw new ArgumentNullException(nameof(componentOfferRepository));
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorComponentOfferCreateCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await componentOfferRepository
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

        public async Task<ICommandResult> HandleAsync(OrchestratorComponentOfferUpdateCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await componentOfferRepository
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

        public async Task<ICommandResult> HandleAsync(OrchestratorComponentOfferDeleteCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await componentOfferRepository
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
