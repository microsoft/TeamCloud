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
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Operations.Activities;
using TeamCloud.Orchestrator.Operations.Orchestrations.Utilities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Commands
{
    public static class ComponentMonitorCommandOrchestration
    {
        [FunctionName(nameof(ComponentMonitorCommandOrchestration))]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var command = context.GetInput<ComponentMonitorCommand>();
            var commandResult = command.CreateResult();

            try
            {
                var component = command.Payload = (await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ComponentId = command.Payload.Id, ProjectId = command.Payload.ProjectId })
                    .ConfigureAwait(true)) ?? command.Payload;

                // we need to call the component guard activity to check if the current component
                // is ready for processing - means parent org and project must be in a ready state
                var ready = await context
                    .CallActivityWithRetryAsync<bool>(nameof(ComponentGuardActivity), new ComponentGuardActivity.Input() { Component = component })
                    .ConfigureAwait(true);

                if (ready)
                {
                    context
                        .StartNewOrchestration(nameof(ComponentMonitorOrchestration), new ComponentMonitorOrchestration.Input() { ComponentId = component.Id, ProjectId = component.ProjectId }, component.Id);
                }
                else
                {
                    await context
                        .ContinueAsNew(command, TimeSpan.FromSeconds(2))
                        .ConfigureAwait(true);
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ComponentDeploymentExecuteCommandOrchestration)} failed: {exc.Message}");

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
