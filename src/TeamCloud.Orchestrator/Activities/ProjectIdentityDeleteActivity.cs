/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Directory;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectIdentityDeleteActivity
    {
        private readonly IAzureDirectoryService azureDirectoryService;
        private readonly IAzureResourceService azureResourceService;

        public ProjectIdentityDeleteActivity(IAzureDirectoryService azureDirectoryService, IAzureResourceService azureResourceService)
        {
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ProjectIdentityDeleteActivity)), RetryOptions(3)]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var project = functionContext.GetInput<ProjectDocument>();

            if (!string.IsNullOrEmpty(project.Identity?.Id))
            {
                await azureDirectoryService
                    .DeleteServicePrincipalAsync(project.Id.ToString())
                    .ConfigureAwait(false);
            }
        }
    }
}
