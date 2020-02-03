/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Orchestrator.Orchestrations.Projects;
using TeamCloud.Orchestrator.Orchestrations.TeamCloud;

namespace TeamCloud.Orchestrator
{
    public class CommandTrigger
    {
        private readonly IProjectsRepository projectsRepository;
        private readonly ITeamCloudRepository teamCloudRepository;

        public CommandTrigger(IProjectsRepository projectsRepository, ITeamCloudRepository teamCloudRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(CommandTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "command")] HttpRequest httpRequest,
            [DurableClient] IDurableClient durableClient
            /* ILogger log */)
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

            if (teamCloud is null)
                throw new NullReferenceException();

            var orchestratorCommand = new OrchestratorCommandMessage(teamCloud, command);

            var commandResult = await SendCommand(durableClient, orchestratorCommand, OrchestrationName(command))
                .ConfigureAwait(false);

            return new OkObjectResult(commandResult);
        }

        private static string OrchestrationName(ICommand command) => (command) switch
        {
            ProjectCreateCommand _ => nameof(ProjectCreateOrchestration),
            ProjectUpdateCommand _ => nameof(ProjectCreateOrchestration),
            ProjectDeleteCommand _ => nameof(ProjectDeleteOrchestration),
            ProjectUserCreateCommand _ => nameof(ProjectUserCreateOrchestration),
            ProjectUserUpdateCommand _ => nameof(ProjectUserCreateOrchestration),
            ProjectUserDeleteCommand _ => nameof(ProjectUserDeleteOrchestration),
            TeamCloudCreateCommand _ => nameof(TeamCloudCreateOrchestration),
            TeamCloudUserCreateCommand _ => nameof(TeamCloudUserCreateOrchestration),
            TeamCloudUserUpdateCommand _ => nameof(TeamCloudUserCreateOrchestration),
            TeamCloudUserDeleteCommand _ => nameof(TeamCloudUserDeleteOrchestration),
            _ => throw new NotSupportedException()
        };

        private static async Task<ICommandResult> SendCommand(IDurableClient durableClient, OrchestratorCommandMessage orchestratorCommand, string orchestrationName)
        {
            var instanceId = await durableClient
                .StartNewAsync<object>(orchestrationName, orchestratorCommand.CommandId.ToString(), orchestratorCommand)
                .ConfigureAwait(false);

            var status = await durableClient
                .GetStatusAsync(instanceId)
                .ConfigureAwait(false);

            return orchestratorCommand.Command.CreateResult(status);
        }
    }
}
