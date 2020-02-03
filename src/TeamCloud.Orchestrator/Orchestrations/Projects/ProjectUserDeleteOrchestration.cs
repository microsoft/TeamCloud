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
    public static class ProjectUserDeleteOrchestration
    {
        [FunctionName(nameof(ProjectUserDeleteOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommand>();

            var command = orchestratorCommand.Command as ProjectUserDeleteCommand;

            await functionContext
                .WaitForProjectCommandsAsync(command)
                .ConfigureAwait(true);

            var user = await functionContext
                .CallActivityAsync<User>(nameof(ProjectUserDeleteActivity), (command.ProjectId, command.Payload))
                .ConfigureAwait(true);

            // TODO: call set users on all providers (or project update for now)

            var commandResult = command.CreateResult();
            commandResult.Result = user;

            functionContext.SetOutput(commandResult);
        }
    }
}
