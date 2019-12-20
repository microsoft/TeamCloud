/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Model;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations
{
    public static class ProjectUserDeleteOrchestration
    {
        [FunctionName(nameof(ProjectUserDeleteOrchestration))]
        public static async Task<Project> RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            (OrchestratorContext orchestratorContext, ProjectUser deleteUser) input = functionContext.GetInput<(OrchestratorContext, ProjectUser)>();

            var project = await functionContext.CallActivityAsync<Project>(nameof(ProjectUserDeleteActivity), (input.orchestratorContext.Project, input.deleteUser));

            var projectContext = new ProjectContext(input.orchestratorContext.TeamCloud, project, input.orchestratorContext.User.Id);

            // TODO: call set users on all providers
            // var tasks = input.teamCloud.Configuration.Providers.Select(p =>
            //                 functionContext.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

            // await Task.WhenAll(tasks);

            return project;
        }
    }
}