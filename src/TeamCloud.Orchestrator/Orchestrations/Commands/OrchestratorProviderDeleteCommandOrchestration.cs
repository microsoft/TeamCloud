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
using TeamCloud.Model.Commands;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProviderDeleteCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProviderDeleteCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = (OrchestratorProviderDeleteCommand)commandMessage.Command;
            var commandResult = command.CreateResult();

            try
            {
                var teamCloud = await functionContext
                    .GetTeamCloudAsync()
                    .ConfigureAwait(true);

                var provider = command.Payload;

                using (await functionContext.LockAsync(teamCloud).ConfigureAwait(true))
                {
                    teamCloud = await functionContext
                        .GetTeamCloudAsync()
                        .ConfigureAwait(true);

                    provider = commandResult.Result = teamCloud.Providers
                        .SingleOrDefault(p => p.Id.Equals(provider.Id, StringComparison.Ordinal));

                    if (provider != null)
                        teamCloud.Providers.Remove(provider);

                    teamCloud = await functionContext
                        .SetTeamCloudAsync(teamCloud)
                        .ConfigureAwait(true);
                }
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
