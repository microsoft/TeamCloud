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
using TeamCloud.Model.Internal.Commands;
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
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorProviderCreateCommand>();
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

                    using (await functionContext.LockContainerDocumentAsync(provider).ConfigureAwait(true))
                    {
                        var existingProvider = await functionContext
                            .GetProviderAsync(provider.Id)
                            .ConfigureAwait(true);

                        if (!(existingProvider is null))
                            throw new OrchestratorCommandException($"Provider {provider.Id} already exists.");

                        functionContext.SetCustomStatus($"Creating provider", log);

                        provider = commandResult.Result = await functionContext
                            .CreateProviderAsync(provider)
                            .ConfigureAwait(true);
                    }

                    functionContext.SetCustomStatus($"Registering provider", log);

                    await functionContext
                        .RegisterProviderAsync(provider, true)
                        .ConfigureAwait(true);
                }
                catch (Exception exc)
                {
                    functionContext.SetCustomStatus($"Handling error: {exc.Message}", log, exc);

                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);
                }
                finally
                {
                    var commandException = commandResult.Errors?.ToException();

                    if (commandException is null)
                        functionContext.SetCustomStatus($"Command succeeded", log);
                    else
                        functionContext.SetCustomStatus($"Command failed: {commandException.Message}", log, commandException);

                    functionContext.SetOutput(commandResult);
                }
            }
        }
    }
}
