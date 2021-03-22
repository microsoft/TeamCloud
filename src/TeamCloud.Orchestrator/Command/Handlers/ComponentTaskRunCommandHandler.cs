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
using TeamCloud.Orchestrator.Command;
using TeamCloud.Orchestrator.Command.Activities;
using TeamCloud.Orchestrator.Command.Activities.Components;
using TeamCloud.Orchestrator.Command.Activities.ComponentTasks;
using TeamCloud.Orchestrator.Command.Entities;
using TeamCloud.Orchestrator.Command.Orchestrations;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ComponentTaskRunCommandHandler : ICommandHandler<ComponentTaskRunCommand>
    {
        public bool Orchestration => true;

        public async Task<ICommandResult> HandleAsync(ComponentTaskRunCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)

        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var commandResult = command.CreateResult();

            commandResult.Result = await orchestrationContext
                .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskGetActivity), new ComponentTaskGetActivity.Input() { ComponentTaskId = command.Payload.Id, ComponentId = command.Payload.ComponentId })
                .ConfigureAwait(true) ?? command.Payload;

            var component = await orchestrationContext
                .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ComponentId = command.Payload.ComponentId, ProjectId = command.Payload.ProjectId })
                .ConfigureAwait(true);

            component = await orchestrationContext
                .CallSubOrchestratorWithRetryAsync<Component>(nameof(ComponentPrepareOrchestration), new ComponentPrepareOrchestration.Input() { Component = component })
                .ConfigureAwait(true);

            using (await orchestrationContext.LockContainerDocumentAsync(component, nameof(ComponentTaskRunCommandHandler)).ConfigureAwait(true))
            {
                try
                {
                    if (string.IsNullOrEmpty(commandResult.Result.ResourceId))
                    {
                        commandResult.Result = await UpdateComponentDeploymentAsync(commandResult.Result, ResourceState.Initializing)
                            .ConfigureAwait(true);

                        commandResult.Result = await orchestrationContext
                            .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskRunnerActivity), new ComponentTaskRunnerActivity.Input() { ComponentTask = commandResult.Result })
                            .ConfigureAwait(true);
                    }

                    commandResult.Result = await UpdateComponentDeploymentAsync(commandResult.Result, ResourceState.Provisioning)
                        .ConfigureAwait(true);

                    while (!commandResult.Result.ResourceState.IsFinal())
                    {
                        // component deployment's TTL is 30 min max
                        if (commandResult.Result.Created.AddMinutes(30) < orchestrationContext.CurrentUtcDateTime) break;

                        await orchestrationContext
                            .CreateTimer(TimeSpan.FromSeconds(10))
                            .ConfigureAwait(true);

                        commandResult.Result = await orchestrationContext
                            .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentDeploymentMonitorActivity), new ComponentDeploymentMonitorActivity.Input() { ComponentTask = commandResult.Result })
                            .ConfigureAwait(true);
                    }
                }
                finally
                {
                    commandResult.Result = await orchestrationContext
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

            return commandResult;

            Task<ComponentTask> UpdateComponentDeploymentAsync(ComponentTask componentTask, ResourceState? resourceState = null)
            {
                componentTask.ResourceState = resourceState.GetValueOrDefault(componentTask.ResourceState);

                return orchestrationContext.CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskSetActivity), new ComponentTaskSetActivity.Input() { ComponentTask = componentTask });
            }
        }
    }
}
