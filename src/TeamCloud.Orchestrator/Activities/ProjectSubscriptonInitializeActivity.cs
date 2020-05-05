/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Deployment;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.API;
using TeamCloud.Orchestrator.Templates;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectSubscriptonInitializeActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;

        public ProjectSubscriptonInitializeActivity(IAzureDeploymentService azureDeploymentService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new System.ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(ProjectSubscriptonInitializeActivity))]
        [RetryOptions(3)]
        public async Task<string> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var subscriptionId = functionContext.GetInput<Guid>();
            var location = azureDeploymentService.Options.DefaultLocation;

            if (string.IsNullOrEmpty(location))
            {
                // we are unable to provision an event grid subscription without a location
                throw new RetryCanceledException($"Missing a default location for Azure deployments");
            }

            var template = new InitializeSubscriptionTemplate();

            template.Parameters["eventGridLocation"] = location;
            template.Parameters["eventGridEndpoint"] = await EventTrigger.GetUrlAsync().ConfigureAwait(false);

            try
            {
                var deployment = await azureDeploymentService
                    .DeploySubscriptionTemplateAsync(template, subscriptionId, location)
                    .ConfigureAwait(false);

                return deployment.ResourceId;
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Activity '{nameof(ProjectResourcesCreateActivity)} failed: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }
}
