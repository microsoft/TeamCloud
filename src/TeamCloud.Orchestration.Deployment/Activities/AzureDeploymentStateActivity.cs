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
using TeamCloud.Serialization;

namespace TeamCloud.Orchestration.Deployment.Activities
{
    public class AzureDeploymentStateActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;

        public AzureDeploymentStateActivity(IAzureDeploymentService azureDeploymentService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(AzureDeploymentStateActivity))]
        [RetryOptions(3)]
        public async Task<AzureDeploymentState> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            ILogger log)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var resourceId = activityContext.GetInput<string>();

            try
            {
                var deployment = await azureDeploymentService
                    .GetAzureDeploymentAsync(resourceId)
                    .ConfigureAwait(false);

                if (deployment is null)
                    throw new NullReferenceException($"Could not find deployment by resource id '{resourceId}'");

                return await deployment
                    .GetStateAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableException))
            {
                log.LogError(exc, $"Activity {nameof(AzureDeploymentStateActivity)} failed: {exc.Message}");

                throw serializableException;
            }
        }
    }

}
