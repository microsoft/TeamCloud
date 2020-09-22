/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Deployment;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestration.Deployment.Activities
{
    public class AzureDeploymentErrorsActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;

        public AzureDeploymentErrorsActivity(IAzureDeploymentService azureDeploymentService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(AzureDeploymentErrorsActivity))]
        [RetryOptions(3)]
        public async Task<IEnumerable<string>> RunActivity(
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
                    .GetErrorsAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log?.LogError(exc, $"Activity {nameof(AzureDeploymentErrorsActivity)} failed: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }

}
