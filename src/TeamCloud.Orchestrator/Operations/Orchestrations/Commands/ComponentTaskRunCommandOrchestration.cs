/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Operations.Activities;
using TeamCloud.Orchestrator.Operations.Entities;
using TeamCloud.Orchestrator.Operations.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Commands
{
    public static class ComponentTaskRunCommandOrchestration
    {
        [FunctionName(nameof(ComponentTaskRunCommandOrchestration))]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var command = context.GetInput<ComponentTaskRunCommand>();
            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await context
                    .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskGetActivity), new ComponentTaskGetActivity.Input() { ComponentTaskId = command.Payload.Id, ComponentId = command.Payload.ComponentId })
                    .ConfigureAwait(true) ?? command.Payload;

                var component = await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ComponentId = command.Payload.ComponentId, ProjectId = command.Payload.ProjectId })
                    .ConfigureAwait(true);

                component = await context
                    .CallSubOrchestratorWithRetryAsync<Component>(nameof(ComponentPrepareOrchestration), new ComponentPrepareOrchestration.Input() { Component = component })
                    .ConfigureAwait(true);

                using (await context.LockContainerDocumentAsync(component, nameof(ComponentTaskRunCommandOrchestration)).ConfigureAwait(true))
                {
                    try
                    {
                        if (string.IsNullOrEmpty(commandResult.Result.ResourceId))
                        {
                            commandResult.Result = await UpdateComponentDeploymentAsync(commandResult.Result, ResourceState.Initializing)
                                .ConfigureAwait(true);

                            commandResult.Result = await context
                                .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskRunnerActivity), new ComponentTaskRunnerActivity.Input() { ComponentTask = commandResult.Result })
                                .ConfigureAwait(true);
                        }

                        commandResult.Result = await UpdateComponentDeploymentAsync(commandResult.Result, ResourceState.Provisioning)
                            .ConfigureAwait(true);

                        while (!commandResult.Result.ResourceState.IsFinal())
                        {
                            // component deployment's TTL is 30 min max
                            if (commandResult.Result.Created.AddMinutes(30) < context.CurrentUtcDateTime) break;

                            await context
                                .CreateTimer(TimeSpan.FromSeconds(10))
                                .ConfigureAwait(true);

                            commandResult.Result = await context
                                .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentDeploymentMonitorActivity), new ComponentDeploymentMonitorActivity.Input() { ComponentTask = commandResult.Result })
                                .ConfigureAwait(true);
                        }
                    }
                    finally
                    {
                        commandResult.Result = await context
                            .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskTerminateActivity), new ComponentTaskTerminateActivity.Input() { ComponentTask = commandResult.Result })
                            .ConfigureAwait(true);
                    }
                }

                if (!commandResult.Result.ResourceState.IsFinal())
                {
                    // the component deployment's resource state wasn't set to a final state by the handler functions.
                    // as there was no exception thrown we assume the processing succeeded an set the appropriate state.

                    commandResult.Result = await UpdateComponentDeploymentAsync(commandResult.Result, ResourceState.Succeeded)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ComponentTaskRunCommandOrchestration)} failed: {exc.Message}");

                commandResult.Errors.Add(exc);
            }
            finally
            {
                context.SetOutput(commandResult);
            }

            Task<ComponentTask> UpdateComponentDeploymentAsync(ComponentTask componentTask, ResourceState? resourceState = null)
            {
                componentTask.ResourceState = resourceState.GetValueOrDefault(componentTask.ResourceState);

                return context.CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskSetActivity), new ComponentTaskSetActivity.Input() { ComponentTask = componentTask });
            }

        }
    }
}
