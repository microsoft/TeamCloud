/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Data;
using TeamCloud.Model;
using TeamCloud.Orchestrator.Orchestrations;

namespace TeamCloud.Orchestrator
{
    public class InvokeOrchestrator
    {
        private readonly IProjectsRepository projectsRepository;
        private readonly ITeamCloudRepository teamCloudRepository;

        public InvokeOrchestrator(IProjectsRepository projectsRepository, ITeamCloudRepository teamCloudRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(InvokeOrchestrator))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orchestrator")] HttpRequest httpRequest,
            [DurableClient] IDurableClient durableClient,
            ILogger logger)
        {
            if (httpRequest is null)
                throw new ArgumentNullException(nameof(httpRequest));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var requestBody = await new StreamReader(httpRequest.Body)
                .ReadToEndAsync()
                .ConfigureAwait(false);

            var command = JsonConvert.DeserializeObject<ICommand>(requestBody);

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var orchestratorContext = new OrchestratorContext(teamCloud);

            if (command.ProjectId.HasValue)
            {
                var project = await projectsRepository
                    .GetAsync(command.ProjectId.Value)
                    .ConfigureAwait(false);

                orchestratorContext.Project = project;
            }

            switch (command)
            {
                case ProjectCreateCommand projectCreateCommand:
                var projectCreateCommandResult = await Handle(durableClient, orchestratorContext, projectCreateCommand, nameof(ProjectCreateOrchestration))
                    .ConfigureAwait(false);
                return new OkObjectResult(projectCreateCommandResult);

                case ProjectUpdateCommand projectUpdateCommand:
                var projectUpdateCommandResult = await Handle(durableClient, orchestratorContext, projectUpdateCommand, nameof(ProjectCreateOrchestration))
                    .ConfigureAwait(false);
                return new OkObjectResult(projectUpdateCommandResult);

                case ProjectDeleteCommand projectDeleteCommand:
                var projectDeleteCommandResult = await Handle(durableClient, orchestratorContext, projectDeleteCommand, nameof(ProjectDeleteOrchestration))
                    .ConfigureAwait(false);
                return new ObjectResult(projectDeleteCommandResult);

                case ProjectUserCreateCommand projectUserCreateCommand:
                var projectUserCreateCommandResult = await Handle(durableClient, orchestratorContext, projectUserCreateCommand, nameof(ProjectUserCreateOrchestration))
                    .ConfigureAwait(false);
                return new OkObjectResult(projectUserCreateCommandResult);

                case ProjectUserUpdateCommand projectUserUpdateCommand:
                var projectUserUpdateCommandResult = await Handle(durableClient, orchestratorContext, projectUserUpdateCommand, nameof(ProjectUserCreateOrchestration))
                    .ConfigureAwait(false);
                return new OkObjectResult(projectUserUpdateCommandResult);

                case ProjectUserDeleteCommand projectUserDeleteCommand:
                var projectUserDeleteCommandResult = await Handle(durableClient, orchestratorContext, projectUserDeleteCommand, nameof(ProjectUserDeleteOrchestration))
                    .ConfigureAwait(false);
                return new OkObjectResult(projectUserDeleteCommandResult);

                case TeamCloudCreateCommand teamCloudCreateCommand:
                var teamCloudCreateCommandResult = await Handle(durableClient, orchestratorContext, teamCloudCreateCommand, nameof(TeamCloudCreateOrchestration))
                    .ConfigureAwait(false);
                return new OkObjectResult(teamCloudCreateCommandResult);

                case TeamCloudUserCreateCommand teamCloudUserCreateCommand:
                var teamCloudUserCreateCommandResult = await Handle(durableClient, orchestratorContext, teamCloudUserCreateCommand, nameof(TeamCloudUserCreateOrchestration))
                    .ConfigureAwait(false);
                return new OkObjectResult(teamCloudUserCreateCommandResult);

                case TeamCloudUserUpdateCommand teamCloudUserUpdateCommand:
                var teamCloudUserUpdateCommandResult = await Handle(durableClient, orchestratorContext, teamCloudUserUpdateCommand, nameof(TeamCloudUserCreateOrchestration))
                    .ConfigureAwait(false);
                return new OkObjectResult(teamCloudUserUpdateCommandResult);

                case TeamCloudUserDeletCommand teamCloudUserDeletCommand:
                var teamCloudUserDeletCommandResult = await Handle(durableClient, orchestratorContext, teamCloudUserDeletCommand, nameof(TeamCloudUserDeleteOrchestration))
                    .ConfigureAwait(false);
                return new OkObjectResult(teamCloudUserDeletCommandResult);
            }

            return new NotFoundResult();
        }


        private async Task<ICommandResult<TResult>> Handle<TPayload, TResult>(IDurableClient durableClient, OrchestratorContext orchestratorContext, ICommand<TPayload, TResult> command, string orchestrationName)
            where TPayload : new()
            where TResult : new()
        {
            var instanceId = await durableClient
                .StartNewAsync<object>(orchestrationName, command.CommandId.ToString(), (orchestratorContext, command))
                .ConfigureAwait(false);

            var status = await durableClient
                .GetStatusAsync(instanceId)
                .ConfigureAwait(false);

            return status.GetResult<TResult>();
        }
    }
}
