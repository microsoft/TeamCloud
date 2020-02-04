/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Providers.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    public static class ProviderRegisterOrchestration
    {
        [FunctionName(nameof(ProviderRegisterOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = orchestratorCommand.Command as ProviderRegisterCommand;

            var provider = command.Payload;
            var teamCloud = orchestratorCommand.TeamCloud;

            functionContext.SetCustomStatus("Registering Providers");

            var providerCommandTasks = teamCloud.GetProviderCommandTasks(command, functionContext);

            var providerCommandResultMessages = await Task
                .WhenAll(providerCommandTasks)
                .ConfigureAwait(true);

            var providerRegistrations = providerCommandResultMessages
                .Select(m => (m.Provider, m.CommandResult))
                .Cast<(Provider provider, ProviderRegisterCommandResult commandResult)>()
                .Select(pr => (pr.provider, pr.commandResult.Result))
                .ToList();

            var providers = await functionContext
                .CallActivityAsync<List<Provider>>(nameof(ProviderRegisterActivity), providerRegistrations)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus("Providers Registered");

            var commandResult = command.CreateResult();

            // TODO: Figure out how to merge these results
            // commandResult.Result = providerCommandResults.FirstOrDefault(cr => cr.Result != null).Result;

            var providerExcepitons = providerCommandResultMessages
                .Where(cr => cr.Exceptions.Any())
                .SelectMany(cr => cr.Exceptions)
                .ToList();

            if (providerExcepitons.Any())
                commandResult.Exceptions.AddRange(providerExcepitons);

            functionContext.SetOutput(commandResult);
        }
    }
}
