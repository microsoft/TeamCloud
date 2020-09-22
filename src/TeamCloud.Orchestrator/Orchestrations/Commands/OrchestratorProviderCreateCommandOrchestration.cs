/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Internal;
using TeamCloud.Model.Commands;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProviderCreateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProviderCreateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestrationContext.GetInput<OrchestratorProviderCreateCommand>();
            var commandResult = command.CreateResult();
            var provider = command.Payload;

            using (log.BeginCommandScope(command, provider))
            {
                try
                {
                    // ensure the new provider is
                    // marked as not registered so we
                    // can start a provider registration
                    // afterwards

                    provider.Registered = null;

                    using (await orchestrationContext.LockContainerDocumentAsync(provider).ConfigureAwait(true))
                    {
                        var existingProvider = await orchestrationContext
                            .GetProviderAsync(provider.Id)
                            .ConfigureAwait(true);

                        if (!(existingProvider is null))
                            throw new OrchestratorCommandException($"Provider {provider.Id} already exists.");

                        orchestrationContext.SetCustomStatus($"Creating provider", log);

                        provider = commandResult.Result = await orchestrationContext
                            .CreateProviderAsync(provider)
                            .ConfigureAwait(true);
                    }

                    orchestrationContext.SetCustomStatus($"Registering provider", log);

                    await orchestrationContext
                        .RegisterProviderAsync(provider, true)
                        .ConfigureAwait(true);
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
                        orchestrationContext.SetCustomStatus($"Command failed: {commandException.Message}", log, commandException);

                    orchestrationContext.SetOutput(commandResult);
                }
            }
        }
    }
}
