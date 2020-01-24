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

            functionContext.SetCustomStatus($"Updating Project {command.ProjectId}...");

            var user = command.User;
            var project = command.Payload;
            var teamCloud = orchestratorContext.TeamCloud;

            // Update project
            teamCloud = await functionContext
                .CallActivityAsync<TeamCloudInstance>(nameof(ProjectUpdateActivity), orchestratorContext.Project)
                .ConfigureAwait(true);

            // Send update command to providers
            var projectContext = new ProjectContext(teamCloud, project, user);
            var providerCommands = teamCloud.Providers.Select(provider => new ProviderCommand { Command = command, Provider = provider });
            // Execute tasks
            var providerCommandTasks = providerCommands.Select(providerCommand => functionContext.CallSubOrchestratorAsync<ProviderCommandResult>(nameof(ProviderCommandOrchestration), providerCommand));
            var providerCommandResults = await Task.WhenAll(providerCommandTasks).ConfigureAwait(true);

            functionContext.SetOutput(project);
        }
    }
}