/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Operations.Activities;
using TeamCloud.Orchestrator.Operations.Activities.Templates;
using TeamCloud.Orchestrator.Operations.Entities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Commands
{
    public static class ProjectDeployCommandOrchestration
    {
        [FunctionName(nameof(ProjectDeployCommandOrchestration))]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var command = context.GetInput<ProjectDeployCommand>();
            var commandResult = command.CreateResult();

            try
            {
                using (await context.LockContainerDocumentAsync(command.Payload).ConfigureAwait(true))
                {
                    // just to make sure we are dealing with the latest version
                    // of the Project entity, we re-fetch the entity and
                    // use the passed in one as a potential fallback.

                    var project = (await context
                        .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input() { Id = command.Payload.Id, Organization = command.Payload.Organization })
                        .ConfigureAwait(true)) ?? command.Payload;

                    try
                    {
                        project = await context
                            .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), new ProjectSetActivity.Input() { Project = project, ResourceState = ResourceState.Initializing })
                            .ConfigureAwait(true);

                        project = await context
                            .CallActivityWithRetryAsync<Project>(nameof(ProjectInitActivity), new ProjectInitActivity.Input() { Project = project })
                            .ConfigureAwait(true);

                        var deploymentOutputEventName = context.NewGuid().ToString();

                        _ = await context
                            .StartDeploymentAsync(nameof(ProjectDeployActivity), new ProjectDeployActivity.Input() { Project = project }, deploymentOutputEventName)
                            .ConfigureAwait(true);

                        project = await context
                            .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), new ProjectSetActivity.Input() { Project = project, ResourceState = ResourceState.Provisioning })
                            .ConfigureAwait(true);

                        _ = await context
                            .WaitForDeploymentOutput(deploymentOutputEventName, TimeSpan.FromMinutes(5))
                            .ConfigureAwait(true);

                        project = await context
                            .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), new ProjectSetActivity.Input() { Project = project, ResourceState = ResourceState.Succeeded })
                            .ConfigureAwait(true);
                    }
                    catch (Exception deploymentExc)
                    {
                        log.LogError(deploymentExc, $"Failed to deploy resources for project {project.Id}: {deploymentExc.Message}");

                        project = await context
                            .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), new ProjectSetActivity.Input() { Project = project, ResourceState = ResourceState.Failed })
                            .ConfigureAwait(true);
                    }
                    finally
                    {
                        commandResult.Result = project;
                    }
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ProjectDeployCommandOrchestration)} failed: {exc.Message}");

                commandResult.Errors.Add(exc);

                throw exc.AsSerializable();
            }
            finally
            {
                context.SetOutput(commandResult);
            }
        }

    }
}
