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
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.Providers;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class ProjectDeleteOrchestration
    {
        [FunctionName(nameof(ProjectDeleteOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommand>();

            var command = orchestratorCommand.Command as ProjectDeleteCommand;

            await functionContext
                .WaitForProjectCommandsAsync(command)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Deleting Project {command.ProjectId}...");

            var user = command.User;
            var project = command.Payload;
            var teamCloud = orchestratorCommand.TeamCloud;

            // Execute tasks in reverse order
            var providerCommands = teamCloud.Providers.Select(provider => new ProviderCommand { Command = command, Provider = provider });
            var providerCommandTasks = providerCommands.Reverse().Select(providerCommand => functionContext.CallSubOrchestratorAsync<ProviderCommandResult>(nameof(ProviderCommandOrchestration), providerCommand));

            var providerCommandResults = await Task
                .WhenAll(providerCommandTasks)
                .ConfigureAwait(true);

            // Delete project
            project = await functionContext
                .CallActivityAsync<Project>(nameof(ProjectDeleteActivity), project)
                .ConfigureAwait(true);

            var commandResult = command.CreateResult();
            commandResult.Result = project;

            functionContext.SetOutput(commandResult);
        }
    }
}
