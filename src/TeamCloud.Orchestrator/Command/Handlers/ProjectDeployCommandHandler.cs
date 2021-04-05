/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Command.Activities.Projects;
using TeamCloud.Orchestrator.Command.Entities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ProjectDeployCommandHandler : CommandHandler,
        ICommandHandler<ProjectDeployCommand>
    {
        public ProjectDeployCommandHandler() : base(true)
        { }

        public async Task<ICommandResult> HandleAsync(ProjectDeployCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var commandResult = command.CreateResult();

            using (await orchestrationContext.LockContainerDocumentAsync(command.Payload).ConfigureAwait(true))
            {
                // just to make sure we are dealing with the latest version
                // of the Project entity, we re-fetch the entity and
                // use the passed in one as a potential fallback.

                commandResult.Result = (await orchestrationContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input() { Id = command.Payload.Id, Organization = command.Payload.Organization })
                    .ConfigureAwait(true)) ?? command.Payload;

                commandResult.Result = await orchestrationContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), new ProjectSetActivity.Input() { Project = commandResult.Result, ResourceState = ResourceState.Initializing })
                    .ConfigureAwait(true);

                try
                {
                    var deploymentOutputEventName = orchestrationContext.NewGuid().ToString();

                    _ = await orchestrationContext
                        .StartDeploymentAsync(nameof(ProjectDeployActivity), new ProjectDeployActivity.Input() { Project = commandResult.Result }, deploymentOutputEventName)
                        .ConfigureAwait(true);

                    commandResult.Result = await orchestrationContext
                        .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), new ProjectSetActivity.Input() { Project = commandResult.Result, ResourceState = ResourceState.Provisioning })
                        .ConfigureAwait(true);

                    var deploymentOutput = await orchestrationContext
                        .WaitForDeploymentOutput(deploymentOutputEventName, TimeSpan.FromMinutes(5))
                        .ConfigureAwait(true);

                    if (deploymentOutput.TryGetValue("projectData", out var projectData) && projectData is JObject projectDataJson)
                    {
                        commandResult.Result = TeamCloudSerialize.MergeObject(projectDataJson.ToString(), commandResult.Result);

                        commandResult.Result = await orchestrationContext
                            .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), new ProjectSetActivity.Input() { Project = commandResult.Result, ResourceState = ResourceState.Succeeded })
                            .ConfigureAwait(true);
                    }
                    else
                    {
                        throw new NullReferenceException($"Deployment output doesn't contain 'projectData' output.");
                    }

                }
                catch
                {
                    commandResult.Result = await orchestrationContext
                        .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), new ProjectSetActivity.Input() { Project = commandResult.Result, ResourceState = ResourceState.Failed })
                        .ConfigureAwait(true);

                    throw;
                }
            }

            return commandResult;
        }

    }
}
