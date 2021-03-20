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
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Operations.Activities;

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
                commandResult.Result = (await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ComponentId = command.Payload.Id, ProjectId = command.Payload.ProjectId })
                    .ConfigureAwait(true)) ?? command.Payload;

                // we need to call the component guard activity to check if the current component
                // is ready for processing - means parent org and project must be in a ready state

                var ready = await context
                    .CallActivityWithRetryAsync<bool>(nameof(ComponentGuardActivity), new ComponentGuardActivity.Input() { Component = commandResult.Result })
                    .ConfigureAwait(true);

                if (ready)
                {
                    context.SetCustomStatus($"Organization and project for component {commandResult.Result} hit 'ready' state");
                }
                else
                {
                    context.SetCustomStatus($"Organization or project for component {commandResult.Result} are not yet in 'ready' state");

                    await context
                        .ContinueAsNew(command, TimeSpan.FromSeconds(5))
                        .ConfigureAwait(true);
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ComponentMonitorCommandOrchestration)} failed: {exc.Message}");

                commandResult.Errors.Add(exc);
            }
            finally
            {
                context.SetOutput(commandResult);
            }
        }
    }
}
