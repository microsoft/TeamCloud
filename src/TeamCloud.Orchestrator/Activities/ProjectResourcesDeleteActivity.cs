/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectResourcesDeleteActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureResourceService azureResourceService;

        public ProjectResourcesDeleteActivity(IAzureDeploymentService azureDeploymentService, IAzureSessionService azureSessionService, IAzureResourceService azureResourceService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ProjectResourcesDeleteActivity)), RetryOptions(3)]
        public Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var project = functionContext.GetInput<Project>();

            var tasks = new Task[]
            {
                DeleteInternalResourceGroupAsync(project),
                DeleteProjectResourceGroupAsync(project)
            };

            return Task.WhenAll(tasks);
        }

        private async Task DeleteProjectResourceGroupAsync(Project project)
        {
            if (string.IsNullOrEmpty(project?.ResourceGroup?.ResourceGroupId))
                return;

            var resourceGroup = await azureResourceService
                .GetResourceGroupAsync(project.ResourceGroup.SubscriptionId, project.ResourceGroup.ResourceGroupName)
                .ConfigureAwait(false);

            await (resourceGroup?.DeleteAsync(true) ?? Task.CompletedTask)
                .ConfigureAwait(false);
        }

        private async Task DeleteInternalResourceGroupAsync(Project project)
        {
            if (string.IsNullOrEmpty(project?.ResourceGroup?.ResourceGroupId))
                return;

            var resourceGroup = await azureResourceService
                .GetResourceGroupAsync(project.ResourceGroup.SubscriptionId, $"{project.ResourceGroup.ResourceGroupName}_Internal")
                .ConfigureAwait(false);

            await (resourceGroup?.DeleteAsync(true) ?? Task.CompletedTask)
                .ConfigureAwait(false);
        }
    }
}
