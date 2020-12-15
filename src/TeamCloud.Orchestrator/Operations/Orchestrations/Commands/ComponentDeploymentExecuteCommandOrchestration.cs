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

                if (string.IsNullOrEmpty(componentDeployment.ResourceId))
                {
                    componentDeployment = await UpdateComponentDeploymentAsync(componentDeployment, ResourceState.Initializing)
                        .ConfigureAwait(true);

                    componentDeployment = await context
                        .CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentRunnerActivity), new ComponentDeploymentRunnerActivity.Input() { ComponentDeployment = componentDeployment })
                        .ConfigureAwait(true);

                    context.ContinueAsNew(command);

                }
                else if (!componentDeployment.ExitCode.HasValue)
                {
                    componentDeployment = await UpdateComponentDeploymentAsync(componentDeployment, ResourceState.Provisioning)
                        .ConfigureAwait(true);

                    componentDeployment = await context
                        .CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentUpdateActivity), new ComponentDeploymentUpdateActivity.Input() { ComponentDeployment = componentDeployment })
                        .ConfigureAwait(true);

                    context.ContinueAsNew(command);
                }
                else
                {
                    componentDeployment = await UpdateComponentDeploymentAsync(componentDeployment, ResourceState.Succeeded)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ComponentDeploymentExecuteCommandOrchestration)} failed: {exc.Message}");

                commandResult.Errors.Add(exc);

                componentDeployment = await UpdateComponentDeploymentAsync(componentDeployment, ResourceState.Failed)
                    .ConfigureAwait(true);

                throw exc.AsSerializable();
            }
            finally
            {
                commandResult.Result = componentDeployment;

                context.SetOutput(commandResult);
            }

            Task<ComponentDeployment> UpdateComponentDeploymentAsync(ComponentDeployment componentDeployment, ResourceState? resourceState = null)
            {
                componentDeployment.ResourceState = resourceState.GetValueOrDefault(componentDeployment.ResourceState);

                return context.CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentSetActivity), new ComponentDeploymentSetActivity.Input() { ComponentDeployment = componentDeployment });
            }
        }
    }
}
