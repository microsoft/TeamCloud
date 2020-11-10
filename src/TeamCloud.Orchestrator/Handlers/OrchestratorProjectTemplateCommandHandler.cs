/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Git.Services;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Handlers
{
    public sealed class OrchestratorProjectTemplateCommandHandler
        : IOrchestratorCommandHandler<OrchestratorProjectTemplateCreateCommand>,
          IOrchestratorCommandHandler<OrchestratorProjectTemplateUpdateCommand>,
          IOrchestratorCommandHandler<OrchestratorProjectTemplateDeleteCommand>
    {
        private readonly IRepositoryService repositoryService;
        private readonly IProjectTemplateRepository projectTemplateRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;

        public OrchestratorProjectTemplateCommandHandler(IRepositoryService repositoryService, IProjectTemplateRepository projectTemplateRepository, IComponentTemplateRepository componentTemplateRepository)
        {
            this.repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
            this.projectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorProjectTemplateCreateCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var commandResult = orchestratorCommand.CreateResult();
            var projectTemplate = orchestratorCommand.Payload;

            try
            {
                projectTemplate = await repositoryService
                    .UpdateProjectTemplateAsync(projectTemplate)
                    .ConfigureAwait(false);

                commandResult.Result = await projectTemplateRepository
                    .AddAsync(projectTemplate)
                    .ConfigureAwait(false);

                var componentTemplates = await repositoryService
                    .GetComponentTemplatesAsync(projectTemplate)
                    .ConfigureAwait(false);

                var componentSaveTasks = componentTemplates
                    .Select(c => componentTemplateRepository.SetAsync(c));

                await Task
                    .WhenAll(componentSaveTasks)
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
