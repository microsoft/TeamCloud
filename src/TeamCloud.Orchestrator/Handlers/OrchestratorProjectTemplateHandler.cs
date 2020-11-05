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
    public sealed class OrchestratorProjectTemplateCommandHandler
        : IOrchestratorCommandHandler<OrchestratorProjectTemplateCreateCommand>,
          IOrchestratorCommandHandler<OrchestratorProjectTemplateUpdateCommand>,
          IOrchestratorCommandHandler<OrchestratorProjectTemplateDeleteCommand>
    {
        private readonly IProjectTemplateRepository projectTemplateRepository;

        public OrchestratorProjectTemplateCommandHandler(IProjectTemplateRepository projectTemplateRepository)
        {
            this.projectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorProjectTemplateCreateCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await projectTemplateRepository
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

        public async Task<ICommandResult> HandleAsync(OrchestratorProjectTemplateUpdateCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await projectTemplateRepository
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

        public async Task<ICommandResult> HandleAsync(OrchestratorProjectTemplateDeleteCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await projectTemplateRepository
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
