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
using TeamCloud.Serialization;

namespace TeamCloud.Orchestration.Deployment.Activities
{
    public class AzureDeploymentDeleteActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;

        public AzureDeploymentDeleteActivity(IAzureDeploymentService azureDeploymentService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(AzureDeploymentDeleteActivity))]
        [RetryOptions(5)]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var resourceId = functionContext.GetInput<string>();

            try
            {
                var deployment = await azureDeploymentService
                    .GetAzureDeploymentAsync(resourceId)
                    .ConfigureAwait(false);

                await (deployment?.DeleteAsync() ?? Task.CompletedTask)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log?.LogError(exc, $"Activity {nameof(AzureDeploymentDeleteActivity)} failed: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }
}

