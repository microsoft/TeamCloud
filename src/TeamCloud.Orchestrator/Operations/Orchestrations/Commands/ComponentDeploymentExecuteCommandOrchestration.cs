/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Operations.Activities;
using TeamCloud.Orchestrator.Operations.Activities.Templates;
using TeamCloud.Orchestrator.Operations.Entities;
using TeamCloud.Orchestrator.Operations.Orchestrations.Utilities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Commands
{
    public static class ComponentDeploymentExecuteCommandOrchestration
    {
        [FunctionName(nameof(ComponentDeploymentExecuteCommandOrchestration))]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var command = context.GetInput<ComponentDeploymentExecuteCommand>();
            var commandResult = command.CreateResult();

            var componentDeployment = await context
                .CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentGetActivity), new ComponentDeploymentGetActivity.Input() { ComponentDeploymentId = command.Payload.Id, ComponentId = command.Payload.ComponentId })
                .ConfigureAwait(true) ?? command.Payload;

            try
            {
                var component = await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ComponentId = command.Payload.ComponentId, ProjectId = command.Payload.ProjectId })
                    .ConfigureAwait(true);

                component = await context
                    .CallSubOrchestratorWithRetryAsync<Component>(nameof(ComponentPrepareOrchestration), new ComponentPrepareOrchestration.Input() { Component = component })
                    .ConfigureAwait(true);

                using (await context.LockContainerDocumentAsync(component, nameof(ComponentDeploymentExecuteCommandOrchestration)).ConfigureAwait(true))
                {
                    var task = componentDeployment.Type switch
                    {
                        ComponentDeploymentType.Template => HandleTemplateDeploymentAsync(context, component, componentDeployment, log),
                        ComponentDeploymentType.Clear => HandleClearDeploymentAsync(context, component, componentDeployment, log),
                        _ => throw new NotSupportedException($"Component deployments of type {componentDeployment.Type} are not supported.")
                    };

                    componentDeployment = await task.ConfigureAwait(true);
                }

                if (!componentDeployment.ResourceState.IsFinal())
                {
                    // the component deployment's resource state wasn't set to a final state by the handler functions.
                    // as there was no exception thrown we assume the processing succeeded an set the appropriate state.

                    componentDeployment = await UpdateComponentDeploymentAsync(context, componentDeployment, ResourceState.Succeeded)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ComponentDeploymentExecuteCommandOrchestration)} failed: {exc.Message}");

                commandResult.Errors.Add(exc);

                componentDeployment = await UpdateComponentDeploymentAsync(context, componentDeployment, ResourceState.Failed)
                    .ConfigureAwait(true);

                throw exc.AsSerializable();
            }
            finally
            {
                commandResult.Result = componentDeployment;

                context.SetOutput(commandResult);
            }


        }

        private static async Task<ComponentDeployment> HandleClearDeploymentAsync(IDurableOrchestrationContext context, Component component, ComponentDeployment componentDeployment, ILogger log)
        {
            var componentResourceId = AzureResourceIdentifier.Parse(component.ResourceId);

            if (string.IsNullOrEmpty(componentResourceId.ResourceGroup))
            {
                throw new NotSupportedException($"Clean operation on resource {componentResourceId} isn't supported yet.");
            }
            else
            {
                var resourceIds = await context
                    .CallActivityWithRetryAsync<string[]>(nameof(ComponentResourcesActivity), new ComponentResourcesActivity.Input() { Component = component })
                    .ConfigureAwait(true);

                if (resourceIds.Any())
                {
                    var instanceId = await context
                        .StartDeploymentAsync(nameof(ComponentCleanActivity), new ComponentCleanActivity.Input() { Component = component })
                        .ConfigureAwait(true);

                    if (!componentDeployment.Started.HasValue)
                    {
                        componentDeployment.Started = context.CurrentUtcDateTime;
                        componentDeployment = await UpdateComponentDeploymentAsync(context, componentDeployment, ResourceState.Provisioning)
                            .ConfigureAwait(true);
                    }

                    var orchestrationInProgress = await IsDeploymentOrchestrationInProgressAsync(instanceId)
                        .ConfigureAwait(true);

                    while (orchestrationInProgress)
                    {
                        await context
                            .CreateTimer(TimeSpan.FromSeconds(2))
                            .ConfigureAwait(true);

                        var output = new StringBuilder(componentDeployment.Output);
                        var outputUpdated = false;

                        var resourceIdsLeft = await context
                            .CallActivityWithRetryAsync<string[]>(nameof(ComponentResourcesActivity), new ComponentResourcesActivity.Input() { Component = component })
                            .ConfigureAwait(true);

                        foreach (var resourceId in resourceIds.Except(resourceIdsLeft))
                        {
                            output.AppendLine($"Deleted: {resourceId}");
                            outputUpdated = true;
                        }

                        if (outputUpdated)
                        {
                            componentDeployment.Output = output.ToString();
                            componentDeployment = await UpdateComponentDeploymentAsync(context, componentDeployment)
                                .ConfigureAwait(true);
                        }

                        resourceIds = resourceIdsLeft;

                        orchestrationInProgress = await IsDeploymentOrchestrationInProgressAsync(instanceId)
                            .ConfigureAwait(true);
                    }
                }

                componentDeployment.ExitCode = 0; // we assume a deployment without issues
                componentDeployment.Finished = context.CurrentUtcDateTime; // the finished timestamp is the fallback for the started
                componentDeployment.Started = componentDeployment.Started.GetValueOrDefault(componentDeployment.Finished.Value);
                componentDeployment.Output ??= "No resources found !!!"; // fallback output message

                componentDeployment = await UpdateComponentDeploymentAsync(context, componentDeployment)
                    .ConfigureAwait(true);
            }

            return componentDeployment;

            async Task<bool> IsDeploymentOrchestrationInProgressAsync(string instanceId)
            {
                var status = await context
                    .CallActivityWithRetryAsync<OrchestrationRuntimeStatus?>(nameof(OrchestrationRuntimeStatusActivity), new OrchestrationRuntimeStatusActivity.Input() { InstanceId = instanceId })
                    .ConfigureAwait(true);

                if (status.HasValue)
                    return !status.Value.IsFinal();

                throw new ArgumentOutOfRangeException($"Orchestration instance ID '{instanceId}' does not exist.");
            }
        }

        private static async Task<ComponentDeployment> HandleTemplateDeploymentAsync(IDurableOrchestrationContext context, Component component, ComponentDeployment componentDeployment, ILogger log)
        {
            if (string.IsNullOrEmpty(componentDeployment.ResourceId))
            {
                componentDeployment = await UpdateComponentDeploymentAsync(context, componentDeployment, ResourceState.Initializing)
                    .ConfigureAwait(true);

                componentDeployment = await context
                    .CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentRunnerActivity), new ComponentDeploymentRunnerActivity.Input() { ComponentDeployment = componentDeployment })
                    .ConfigureAwait(true);
            }

            componentDeployment = await UpdateComponentDeploymentAsync(context, componentDeployment, ResourceState.Provisioning)
                .ConfigureAwait(true);

            while (!componentDeployment.ResourceState.IsFinal())
            {
                await context
                    .CreateTimer(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(true);

                componentDeployment = await context
                    .CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentMonitorActivity), new ComponentDeploymentMonitorActivity.Input() { ComponentDeployment = componentDeployment })
                    .ConfigureAwait(true);
            }

            return componentDeployment;
        }

        private static Task<ComponentDeployment> UpdateComponentDeploymentAsync(IDurableOrchestrationContext context, ComponentDeployment componentDeployment, ResourceState? resourceState = null)
        {
            componentDeployment.ResourceState = resourceState.GetValueOrDefault(componentDeployment.ResourceState);

            return context.CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentSetActivity), new ComponentDeploymentSetActivity.Input() { ComponentDeployment = componentDeployment });
        }
    }
}
