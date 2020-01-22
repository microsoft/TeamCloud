/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Context;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class ProjectDeleteOrchestration
    {
        [FunctionName(nameof(ProjectDeleteOrchestration))]
        public static async Task<bool> RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            (OrchestratorContext orchestratorContext, ProjectDeleteCommand command) = functionContext.GetInput<(OrchestratorContext, ProjectDeleteCommand)>();

            await functionContext
                .WaitForProjectCommandsAsync(command)
                .ConfigureAwait(true);


            var teamCloud = await functionContext
                .CallActivityAsync<TeamCloudInstance>(nameof(ProjectDeleteActivity), orchestratorContext.Project)
                .ConfigureAwait(true);

            // TODO: call delete on all providers (handeling dependencies in reverse order??)
            // var tasks = teamCloud.Configuration.Providers.Select(p =>
            //                 functionContext.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

            // await Task.WhenAll(tasks);


            return true;
        }
    }
}