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
using TeamCloud.Orchestrator.Options;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public class OrchestratorProviderDeleteCommandOrchestration
    {
        private readonly TeamCloudDatabaseOptions orchestratorDatabaseOptions;

        public OrchestratorProviderDeleteCommandOrchestration(TeamCloudDatabaseOptions orchestratorDatabaseOptions)
        {
            this.orchestratorDatabaseOptions = orchestratorDatabaseOptions ?? throw new ArgumentNullException(nameof(orchestratorDatabaseOptions));
        }

        [FunctionName(nameof(OrchestratorProviderDeleteCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestrationContext.GetInput<OrchestratorProviderDeleteCommand>();
            var commandResult = command.CreateResult();
            var provider = command.Payload;

            using (log.BeginCommandScope(command, provider))
            {
                try
                {
                    using (await orchestrationContext.LockContainerDocumentAsync(provider).ConfigureAwait(true))
                    {
                        if (!(provider is null))
                        {
                            await orchestrationContext
                                .DeleteProviderAsync(provider)
                                .ConfigureAwait(true);

                            if (provider.PrincipalId.HasValue)
                            {
                                await orchestrationContext
                                    .DeleteUserAsync(provider.PrincipalId.Value.ToString(), allowUnsafe: true)
                                    .ConfigureAwait(true);
                            }
                        }
                    }
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
