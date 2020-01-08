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
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Model;
using TeamCloud.Orchestrator.Orchestrations;

namespace TeamCloud.Orchestrator
{
    public class InvokeOrchestrator
    {
        [FunctionName(nameof(InvokeOrchestrator))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orchestrator")] HttpRequest httpRequest,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(TeamCloudInstance), Id = Constants.CosmosDb.TeamCloudInstanceId, PartitionKey = Constants.CosmosDb.TeamCloudInstanceId, ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloudInstance teamCloud,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(Project), ConnectionStringSetting = "AzureCosmosDBConnection")] DocumentClient documentClient,
            [DurableClient] IDurableClient durableClient,
            ILogger logger)
        {
            var requestBody = await new StreamReader(httpRequest.Body).ReadToEndAsync();

            var command = JsonConvert.DeserializeObject<ICommand>(requestBody);

            Project project = null;

            if (command.ProjectId.HasValue)
            {
                var projectUri = UriFactory.CreateDocumentUri(Constants.CosmosDb.DatabaseName, nameof(Project), command.ProjectId.Value.ToString());

                try
                {
                    var projectResponse = await documentClient.ReadDocumentAsync<Project>(projectUri, new RequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId) });

                    project = projectResponse.Document;
                }
                catch
                {
                    // TODO:
                }
            }

            var user = teamCloud?.Users?.FirstOrDefault(u => u.Id == command.UserId) ?? project?.Users?.FirstOrDefault(u => u.Id == command.UserId);

            var orchestratorContext = new OrchestratorContext(teamCloud, project, user);

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
                .StartNewAsync<object>(orchestrationName, (orchestratorContext, command.Payload))
                .ConfigureAwait(false);

            var status = await durableClient
                .GetStatusAsync(instanceId)
                .ConfigureAwait(false);

            return GetResult<TResult>(status);
        }


        private ICommandResult<TResult> GetResult<TResult>(DurableOrchestrationStatus orchestrationStatus)
            where TResult : new()
        {
            var result = new CommandResult<TResult>(Guid.Parse(orchestrationStatus.InstanceId))
            {
                CreatedTime = orchestrationStatus.CreatedTime,
                LastUpdatedTime = orchestrationStatus.LastUpdatedTime,
                RuntimeStatus = (CommandRuntimeStatus)orchestrationStatus.RuntimeStatus,
                CustomStatus = orchestrationStatus.CustomStatus?.ToString(),
            };

            if (orchestrationStatus.Output?.HasValues ?? false)
            {
                result.Result = orchestrationStatus.Output.ToObject<TResult>();
            }

            return result;
        }
    }
}
