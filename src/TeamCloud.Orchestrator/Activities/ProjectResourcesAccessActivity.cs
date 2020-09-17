/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectResourcesAccessActivity
    {
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureResourceService azureResourceService;
        private readonly IProjectRepository projectsRepository;

        public ProjectResourcesAccessActivity(IAzureSessionService azureSessionService, IAzureResourceService azureResourceService, IProjectRepository projectsRepository)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectResourcesAccessActivity))]
        [RetryOptions(3)]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (projectId, principalId) = functionContext.GetInput<(string, Guid)>();

            var project = await projectsRepository
                .GetAsync(projectId)
                .ConfigureAwait(false);

            if (project is null)
                throw new RetryCanceledException($"Could not find project '{projectId}'");

            if (!string.IsNullOrEmpty(project.ResourceGroup?.Id))
            {
                var resourceGroup = await azureResourceService
                     .GetResourceGroupAsync(project.ResourceGroup.SubscriptionId, project.ResourceGroup.Name)
                     .ConfigureAwait(false);

                if (resourceGroup != null)
                {
                    var roleAssignmentTasks = new List<Task>()
                    {
                        // ensure the given principal id has access to the projects resource group
                        resourceGroup.AddRoleAssignmentAsync(principalId.ToString(), AzureRoleDefinition.Contributor),
                        resourceGroup.AddRoleAssignmentAsync(principalId.ToString(), AzureRoleDefinition.UserAccessAdministrator) // TODO: do we really need this ???
                    };

                    if (!string.IsNullOrEmpty(project.Identity?.Id))
                    {
                        // ensure the project identity has access to the projects resource group
                        roleAssignmentTasks.Add(resourceGroup.AddRoleAssignmentAsync(project.Identity.Id, AzureRoleDefinition.Reader));
                    }

                    await Task
                        .WhenAll(roleAssignmentTasks)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}
