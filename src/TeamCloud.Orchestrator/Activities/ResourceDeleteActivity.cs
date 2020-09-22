/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public class ResourceDeleteActivity
    {
        private readonly IAzureResourceService azureResourceService;

        public ResourceDeleteActivity(IAzureResourceService azureResourceService)
        {
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ResourceDeleteActivity)), RetryOptions(3)]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            ILogger log)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var resourceId = activityContext.GetInput<string>();

            try
            {
                var resource = await azureResourceService
                    .GetResourceAsync(resourceId)
                    .ConfigureAwait(false);

                await (resource?.DeleteAsync(true) ?? Task.CompletedTask)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Activity '{nameof(ResourceDeleteActivity)} failed: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }
}
