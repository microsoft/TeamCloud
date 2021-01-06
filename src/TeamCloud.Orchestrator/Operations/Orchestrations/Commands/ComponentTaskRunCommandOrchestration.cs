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
using TeamCloud.Orchestrator.Operations.Activities;
using TeamCloud.Orchestrator.Operations.Entities;
using TeamCloud.Orchestrator.Operations.Orchestrations.Utilities;
using TeamCloud.Serialization;

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

            var componentTask = await context
                .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskGetActivity), new ComponentTaskGetActivity.Input() { ComponentTaskId = command.Payload.Id, ComponentId = command.Payload.ComponentId })
                .ConfigureAwait(true) ?? command.Payload;

            try
            {
                var component = await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ComponentId = command.Payload.ComponentId, ProjectId = command.Payload.ProjectId })
                    .ConfigureAwait(true);

                component = await context
                    .CallSubOrchestratorWithRetryAsync<Component>(nameof(ComponentPrepareOrchestration), new ComponentPrepareOrchestration.Input() { Component = component })
                    .ConfigureAwait(true);

                using (await context.LockContainerDocumentAsync(component, nameof(ComponentTaskRunCommandOrchestration)).ConfigureAwait(true))
                {
                    if (string.IsNullOrEmpty(componentTask.ResourceId))
                    {
                        componentTask = await UpdateComponentTaskAsync(componentTask, ResourceState.Initializing)
                            .ConfigureAwait(true);

                        componentTask = await context
                            .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskRunnerActivity), new ComponentTaskRunnerActivity.Input() { ComponentTask = componentTask })
                            .ConfigureAwait(true);
                    }

                    componentTask = await UpdateComponentTaskAsync(componentTask, ResourceState.Provisioning)
                        .ConfigureAwait(true);

                    while (!componentTask.ResourceState.IsFinal())
                    {
                        await context
                            .CreateTimer(TimeSpan.FromSeconds(2))
                            .ConfigureAwait(true);

                        componentTask = await context
                            .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskMonitorActivity), new ComponentTaskMonitorActivity.Input() { ComponentTask = componentTask })
                            .ConfigureAwait(true);
                    }
                }

                if (!componentTask.ResourceState.IsFinal())
                {
                    // the component task's resource state wasn't set to a final state by the handler functions.
                    // as there was no exception thrown we assume the processing succeeded an set the appropriate state.

                    componentTask = await UpdateComponentTaskAsync(componentTask, ResourceState.Succeeded)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ComponentTaskRunCommandOrchestration)} failed: {exc.Message}");

                commandResult.Errors.Add(exc);

                componentTask = await UpdateComponentTaskAsync(componentTask, ResourceState.Failed)
                    .ConfigureAwait(true);

                throw exc.AsSerializable();
            }
            finally
            {
                commandResult.Result = componentTask;

                context.SetOutput(commandResult);
            }

            Task<ComponentTask> UpdateComponentTaskAsync(ComponentTask componentTask, ResourceState? resourceState = null)
            {
                componentTask.ResourceState = resourceState.GetValueOrDefault(componentTask.ResourceState);

                return context.CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskSetActivity), new ComponentTaskSetActivity.Input() { ComponentTask = componentTask });
            }
        }
    }
}
