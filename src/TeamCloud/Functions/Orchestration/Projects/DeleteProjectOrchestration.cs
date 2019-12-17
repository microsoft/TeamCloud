/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TeamCloud
{
    public static class DeleteProjectOrchestration
    {
        private static TeamCloudConfiguraiton TeamCloudConfiguraiton = new ConfigurationBuilder()
            .AddAzureAppConfiguration(Environment.GetEnvironmentVariable("AppConfigurationConnectionString"))
            .Build().GetSection("teamcloud").Get<TeamCloudConfiguraiton>();

        [FunctionName(nameof(DeleteProjectOrchestration))]
        public static async Task<bool> RunOrchestration([OrchestrationTrigger] IDurableOrchestrationContext functionContext, ILogger log)
        {
            OrchestratorContext orchestratorContext = functionContext.GetInput<OrchestratorContext>();

            var teamCloud = await functionContext.CallActivityAsync<TeamCloud>(nameof(DeleteProjectActivity), orchestratorContext.Project);

            // TODO: call delete on all providers (handeling dependencies in reverse order??)
            // var tasks = teamCloud.Configuration.Providers.Select(p =>
            //                 functionContext.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

            // await Task.WhenAll(tasks);


            return true;
        }
    }
}