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

            (OrchestratorContext orchestratorContext, ProjectDeleteCommand command) = functionContext.GetInput<(OrchestratorContext, ProjectDeleteCommand)>();

            await functionContext
                .WaitForProjectCommandsAsync(command)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Deleting Project {command.ProjectId}...");

            var user = command.User;
            var project = command.Payload;
            var teamCloud = orchestratorContext.TeamCloud;

            // Send delete command to providers
            var projectContext = new ProjectContext(teamCloud, project, user);
            var providerCommands = teamCloud.Providers.Select(provider => new ProviderCommand { Command = command, Provider = provider });
            // Execute tasks in reverse order
            var providerCommandTasks = providerCommands.Reverse().Select(providerCommand => functionContext.CallSubOrchestratorAsync<ProviderCommandResult>(nameof(ProviderCommandOrchestration), providerCommand));
            var providerCommandResults = await Task.WhenAll(providerCommandTasks).ConfigureAwait(true);

            // Delete project
            teamCloud = await functionContext
                .CallActivityAsync<TeamCloudInstance>(nameof(ProjectDeleteActivity), orchestratorContext.Project)
                .ConfigureAwait(true);

            functionContext.SetOutput(project);
        }
    }
}