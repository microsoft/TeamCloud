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
using TeamCloud.Orchestrator.Operations.Entities;
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
                // we lock the component entity provided as payload 
                // to avoid concurrent deploy / reset operations

                using (await context.LockContainerDocumentAsync(command.Payload).ConfigureAwait(true))
                {
                    var component = (await context
                            .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { Id = command.Payload.Id, Organization = command.Payload.Organization })
                            .ConfigureAwait(true)) ?? command.Payload;

                    var commandResultTask = component.Type switch
                    {
                        ComponentType.Environment => DeployEnvironmentAsync(context, command, log),
                        _ => throw new NotSupportedException($"Component of type '{component.Type}' is not supported.")
                    };

                    commandResult = await commandResultTask.ConfigureAwait(true);
                }
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

        private static Task<ComponentDeployCommandResult> DeployEnvironmentAsync(IDurableOrchestrationContext context, ComponentDeployCommand command, ILogger log)
        {
            // resolve deployment scope defined by component

            // initialize deployment scope for project context

            // allocate component deployment target from scope

            // start component deployment

            // offload component deployment monitoring

            return Task.FromResult(command.CreateResult());
        }
    }
}
