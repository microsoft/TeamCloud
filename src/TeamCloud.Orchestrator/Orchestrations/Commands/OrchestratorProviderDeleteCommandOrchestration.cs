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
    public static class OrchestratorProviderDeleteCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProviderDeleteCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorProviderDeleteCommand>();
            var commandResult = command.CreateResult();
            var provider = command.Payload;

            using (log.BeginCommandScope(command, provider))
            {
                try
                {
                    using (await functionContext.LockAsync<TeamCloudInstance>(TeamCloudInstance.DefaultId).ConfigureAwait(true))
                    {
                        var teamCloud = await functionContext
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
}
