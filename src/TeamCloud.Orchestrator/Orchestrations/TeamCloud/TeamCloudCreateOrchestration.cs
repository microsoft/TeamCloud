/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud
{
    public class TeamCloudCreateOrchestration
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudCreateOrchestration(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new System.ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(TeamCloudCreateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = orchestratorCommand.Command as TeamCloudCreateCommand;

            var teamCloud = await functionContext
                .CallActivityAsync<TeamCloudInstance>(nameof(TeamCloudCreateActivity), command.Payload)
                .ConfigureAwait(true);

            var commandResult = command.CreateResult();
            commandResult.Result = teamCloud;

            functionContext.SetOutput(commandResult);
        }
    }
}
