using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Operations.Entities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Commands
{
    public static class ComponentResetCommandOrchestration
    {
        [FunctionName(nameof(ComponentResetCommandOrchestration))]
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
                    var commandResultTask = command.Payload.Type switch
                    {
                        ComponentType.Environment => ResetEnvironmentAsync(context, command, log),
                        _ => throw new NotSupportedException($"Component of type '{command.Payload.Type}' is not supported.")
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

        private static Task<ComponentDeployCommandResult> ResetEnvironmentAsync(IDurableOrchestrationContext context, ComponentDeployCommand command, ILogger log)
        {
            // get allocated component deployment target

            // if target is resource group - reset resource group

            // if target is subscription - reset and delete all resource groups

            return Task.FromResult(command.CreateResult());
        }
    }
}
