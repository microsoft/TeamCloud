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
using TeamCloud.Model.Context;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations
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

            (OrchestratorContext orchestratorContext, TeamCloudCreateCommand command) = functionContext.GetInput<(OrchestratorContext, TeamCloudCreateCommand)>();

            var teamCloud = await functionContext
                .CallActivityAsync<TeamCloudInstance>(nameof(TeamCloudCreateActivity), command.Payload)
                .ConfigureAwait(true);

            functionContext.SetOutput(teamCloud);
        }
    }
}