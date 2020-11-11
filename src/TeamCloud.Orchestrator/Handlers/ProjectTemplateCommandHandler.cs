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
    public sealed class ProjectTemplateCommandHandler
        : ICommandHandler<ProjectTemplateCreateCommand>,
          ICommandHandler<ProjectTemplateUpdateCommand>,
          ICommandHandler<ProjectTemplateDeleteCommand>
    {
        private readonly IRepositoryService repositoryService;
        private readonly IProjectTemplateRepository projectTemplateRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;

        public ProjectTemplateCommandHandler(IRepositoryService repositoryService, IProjectTemplateRepository projectTemplateRepository, IComponentTemplateRepository componentTemplateRepository)
        {
            this.repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
            this.projectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
        }

        public async Task<ICommandResult> HandleAsync(ProjectTemplateCreateCommand command, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();
            var projectTemplate = command.Payload;

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

        public async Task<ICommandResult> HandleAsync(ProjectTemplateUpdateCommand command, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await projectTemplateRepository
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

        public async Task<ICommandResult> HandleAsync(ProjectTemplateDeleteCommand command, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();

            try
            {
                var componentTemplates = await componentTemplateRepository
                    .ListAsync(command.OrganizationId, command.Payload.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);

                commandResult.Result = await projectTemplateRepository
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
