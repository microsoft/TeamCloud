/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;

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

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = (OrchestratorProviderCreateCommand)commandMessage.Command;
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

                    using (await functionContext.LockAsync<TeamCloudInstance>(TeamCloudInstance.DefaultId).ConfigureAwait(true))
                    {
                        var teamCloud = await functionContext
                            .GetTeamCloudAsync()
                            .ConfigureAwait(true);

                        if (teamCloud.Providers.Any(p => p.Id.Equals(provider.Id, StringComparison.Ordinal)))
                            throw new OrchestratorCommandException($"Provider {provider.Id} already exists.");

                        teamCloud.Providers.Add(provider);

                        teamCloud = await functionContext
                            .SetTeamCloudAsync(teamCloud)
                            .ConfigureAwait(true);

                        provider = commandResult.Result = teamCloud.Providers
                            .Single(p => p.Id.Equals(provider.Id, StringComparison.Ordinal));
                    }

                    await functionContext
                        .RegisterProviderAsync(provider)
                        .ConfigureAwait(true);
                }
                catch (Exception exc)
                {
                    commandResult.Errors.Add(exc);
                }
                finally
                {
                    functionContext.SetOutput(commandResult);
                }
            }
        }
    }
}
