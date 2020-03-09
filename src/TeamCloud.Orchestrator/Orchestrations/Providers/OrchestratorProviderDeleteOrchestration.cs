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
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud
{
    public static class OrchestratorProviderDeleteOrchestration
    {
        [FunctionName(nameof(OrchestratorProviderDeleteOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = (OrchestratorProviderDeleteCommand)commandMessage.Command;
            var commandResult = command.CreateResult();

            try
            {
                var teamCloud = await functionContext
                    .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudGetActivity), null)
                    .ConfigureAwait(true);

                var provider = command.Payload;

                using (await functionContext.LockAsync(teamCloud).ConfigureAwait(true))
                {
                    teamCloud = await functionContext
                        .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudGetActivity), null)
                        .ConfigureAwait(true);

                    provider = commandResult.Result = teamCloud.Providers
                        .SingleOrDefault(p => p.Id.Equals(provider.Id, StringComparison.Ordinal));

                    if (provider != null)
                        teamCloud.Providers.Remove(provider);

                    teamCloud = await functionContext
                        .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudSetActivity), teamCloud)
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
