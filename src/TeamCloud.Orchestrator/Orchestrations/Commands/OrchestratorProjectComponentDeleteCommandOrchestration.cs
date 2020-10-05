/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Internal;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProjectComponentDeleteCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectComponentDeleteCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestrationContext.GetInput<OrchestratorProjectComponentDeleteCommand>();
            var commandResult = command.CreateResult();
            var component = command.Payload;

            using (log.BeginCommandScope(command))
            {
                try
                {
                    orchestrationContext.SetCustomStatus("Getting provider", log);

                    var provider = await orchestrationContext
                        .GetProviderAsync(component.ProviderId, allowUnsafe: true)
                        .ConfigureAwait(true);

                    orchestrationContext.SetCustomStatus("Sending commands", log);

                    var providerCommand = new ProviderComponentDeleteCommand
                    (
                        command.User.PopulateExternalModel(),
                        component.PopulateExternalModel(),
                        command.ProjectId,
                        command.CommandId
                    );

                    var providerResult = await orchestrationContext
                        .SendProviderCommandAsync<ProviderComponentDeleteCommand, ProviderComponentDeleteCommandResult>(providerCommand, provider)
                        .ConfigureAwait(true);

                    providerResult.Errors.ToList().ForEach(e => commandResult.Errors.Add(e));

                    commandResult.Result = commandResult.Result.PopulateFromExternalModel(providerResult.Result);

                    orchestrationContext.SetCustomStatus($"Component delete.", log);
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);
                }
                finally
                {
                    var commandException = commandResult.Errors?.ToException();

                    if (commandException is null)
                        orchestrationContext.SetCustomStatus($"Command succeeded", log);
                    else
                        orchestrationContext.SetCustomStatus($"Command failed", log, commandException);

                    orchestrationContext.SetOutput(commandResult);
                }
            }
        }
    }
}
