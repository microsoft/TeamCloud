/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProviderUpdateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProviderUpdateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = (OrchestratorProviderUpdateCommand)commandMessage.Command;
            var commandResult = command.CreateResult();

            try
            {
                // ensure the updated provider is
                // marked as not registered so we
                // can start a provider registration
                // afterwards

                var provider = command.Payload;

                provider.Registered = null;

                using (await functionContext.LockAsync<TeamCloudInstance>(TeamCloudInstance.DefaultId).ConfigureAwait(true))
                {
                    var teamCloud = await functionContext
                        .GetTeamCloudAsync()
                        .ConfigureAwait(true);

                    var existingProvider = teamCloud.Providers
                        .SingleOrDefault(p => p.Id.Equals(provider.Id, StringComparison.Ordinal));

                    if (existingProvider != null)
                        teamCloud.Providers.Remove(existingProvider);

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
