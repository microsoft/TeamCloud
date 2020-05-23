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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Orchestrator.Options;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public class OrchestratorProviderCreateCommandOrchestration
    {
        private readonly OrchestratorDatabaseOptions orchestratorDatabaseOptions;

        public OrchestratorProviderCreateCommandOrchestration(OrchestratorDatabaseOptions orchestratorDatabaseOptions)
        {
            this.orchestratorDatabaseOptions = orchestratorDatabaseOptions ?? throw new ArgumentNullException(nameof(orchestratorDatabaseOptions));
        }

        [FunctionName(nameof(OrchestratorProviderCreateCommandOrchestration))]
        public async Task RunOrchestration(
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

                    using (await functionContext.LockAsync<TeamCloudInstance>(orchestratorDatabaseOptions.TenantName).ConfigureAwait(true))
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
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);

                    throw;
                }
                finally
                {
                    functionContext.SetOutput(commandResult);
                }
            }
        }
    }
}
