/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Directory;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectIdentityDeleteActivity
    {
        private readonly IAzureDirectoryService azureDirectoryService;

        public ProjectIdentityDeleteActivity(IAzureDirectoryService azureDirectoryService)
        {
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
        }

        [FunctionName(nameof(ProjectIdentityDeleteActivity)), RetryOptions(3)]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            try
            {
                var project = functionContext.GetInput<ProjectDocument>();

                if (!string.IsNullOrEmpty(project.Identity?.Id))
                {
                    await azureDirectoryService
                        .DeleteServicePrincipalAsync(project.Id.ToString())
                        .ConfigureAwait(false);
                }

            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ProjectIdentityDeleteActivity)} failed with error: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }
}
