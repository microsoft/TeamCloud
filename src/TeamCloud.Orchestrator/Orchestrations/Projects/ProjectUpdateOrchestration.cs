/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;

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

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = orchestratorCommand.Command as ProjectUpdateCommand;

            await functionContext
                .WaitForProjectCommandsAsync(command)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Updating Project {command.ProjectId}...");

            var user = command.User;
            var project = command.Payload;
            var teamCloud = orchestratorCommand.TeamCloud;

            project = await functionContext
                .CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project)
                .ConfigureAwait(true);

            var providerCommandTasks = teamCloud.GetProviderCommandTasks(command, functionContext);

            var providerCommandResultMessages = await Task
                .WhenAll(providerCommandTasks)
                .ConfigureAwait(true);

            var commandResult = command.CreateResult();
            commandResult.Result = project;

            functionContext.SetOutput(commandResult);
        }
    }
}
