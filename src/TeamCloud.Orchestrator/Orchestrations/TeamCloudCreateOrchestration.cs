/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model;
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
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            (OrchestratorContext orchestratorContext, TeamCloudCreateCommand command) = functionContext.GetInput<(OrchestratorContext, TeamCloudCreateCommand)>();

            var teamCloud = await functionContext.CallActivityAsync<TeamCloudInstance>(nameof(TeamCloudCreateActivity), command.Payload);

            functionContext.SetOutput(teamCloud);
        }
    }
}