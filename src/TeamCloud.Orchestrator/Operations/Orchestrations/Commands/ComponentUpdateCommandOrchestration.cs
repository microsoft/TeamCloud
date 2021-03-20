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
using TeamCloud.Orchestrator.Operations.Entities;
using TeamCloud.Orchestrator.Operations.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Commands
{
    public static class ComponentUpdateCommandOrchestration
    {
        [FunctionName(nameof(ComponentUpdateCommandOrchestration))]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var command = context.GetInput<ComponentUpdateCommand>();
            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await context
                    .CallSubOrchestratorWithRetryAsync<Component>(nameof(ComponentPrepareOrchestration), new ComponentPrepareOrchestration.Input() { Component = command.Payload })
                    .ConfigureAwait(true);

                using (await context.LockContainerDocumentAsync(commandResult.Result, nameof(ComponentUpdateCommandOrchestration)).ConfigureAwait(true))
                {
                    commandResult.Result = await context
                        .CallActivityWithRetryAsync<Component>(nameof(ComponentEnsurePermissionActivity), new ComponentEnsurePermissionActivity.Input() { Component = commandResult.Result })
                        .ConfigureAwait(true);

                    commandResult.Result = await context
                        .CallActivityWithRetryAsync<Component>(nameof(ComponentEnsureTaggingActivity), new ComponentEnsureTaggingActivity.Input() { Component = commandResult.Result })
                        .ConfigureAwait(true);
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ComponentUpdateCommandOrchestration)} failed: {exc.Message}");

                commandResult.Errors.Add(exc);
            }
            finally
            {
                context.SetOutput(commandResult);
            }
        }
    }
}
