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
using TeamCloud.Model.Context;
using TeamCloud.Orchestrator.Orchestrations.Providers;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class ProjectUpdateOrchestration
    {
        [FunctionName(nameof(ProjectUpdateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            (OrchestratorContext orchestratorContext, ProjectUpdateCommand command) = functionContext.GetInput<(OrchestratorContext, ProjectUpdateCommand)>();

            await functionContext
                .WaitForProjectCommandsAsync(command)
                .ConfigureAwait(true);

            try
            {
                var providerCommands = orchestratorContext.TeamCloud.Providers.Select(provider => new ProviderCommand
                {
                    Command = command,
                    Provider = provider
                });

                var providerCommandResults = await Task.WhenAll(providerCommands
                    .Select(command => functionContext.CallSubOrchestratorAsync<ProviderCommandResult>(nameof(ProviderCommandOrchestration), command)))
                    .ConfigureAwait(true);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
