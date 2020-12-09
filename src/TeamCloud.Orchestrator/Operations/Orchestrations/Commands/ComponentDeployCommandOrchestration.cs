/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
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
    public static class ComponentDeployCommandOrchestration
    {
        [FunctionName(nameof(ComponentDeployCommandOrchestration))]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var command = context.GetInput<ComponentDeployCommand>();
            var commandResult = command.CreateResult();

            try
            {
                command.Payload = (await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { Id = command.Payload.Id, ProjectId = command.Payload.ProjectId })
                    .ConfigureAwait(true)) ?? command.Payload;

                var ready = await context
                    .CallActivityWithRetryAsync<bool>(nameof(ComponentGuardActivity), new ComponentGuardActivity.Input() { Component = command.Payload })
                    .ConfigureAwait(true);

                if (!ready)
                {
                    context
                        .CreateReplaySafeLogger(log)
                        .LogInformation($"!!! Component '{command.Payload.Slug}' needs to wait for Organization/Project resource to be provisioned.");

                    await context
                        .CreateTimer(context.CurrentUtcDateTime.AddSeconds(2), CancellationToken.None)
                        .ConfigureAwait(true);

                    context.ContinueAsNew(command);
                }

                var commandResultTask = command.Payload.Type switch
                {
                    ComponentType.Environment => DeployEnvironmentAsync(context, command.Payload, log),

                    _ => throw new NotSupportedException($"Component of type '{command.Payload.Type}' is not supported.")
                };

                commandResult.Result = await commandResultTask.ConfigureAwait(true);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ComponentDeployCommandOrchestration)} failed: {exc.Message}");

                commandResult.Errors.Add(exc);

                throw exc.AsSerializable();
            }
            finally
            {
                context.SetOutput(commandResult);
            }
        }

        private static async Task<Component> DeployEnvironmentAsync(IDurableOrchestrationContext context, Component component, ILogger log)
        {
            component = await context
                .CallSubOrchestratorWithRetryAsync<Component>(nameof(EnvironmentDeployOrchestration), new EnvironmentDeployOrchestration.Input() { Component = component })
                .ConfigureAwait(true);

            return component;
        }
    }
}
