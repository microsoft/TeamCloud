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
using TeamCloud.Serialization;

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
                var component = command.Payload;

                component = await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentEnsurePermissionActivity), new ComponentEnsurePermissionActivity.Input() { Component = component })
                    .ConfigureAwait(true);

                component = await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentEnsureTaggingActivity), new ComponentEnsureTaggingActivity.Input() { Component = component })
                    .ConfigureAwait(true);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ComponentUpdateCommandOrchestration)} failed: {exc.Message}");

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
