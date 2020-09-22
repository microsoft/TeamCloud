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
            [ActivityTrigger] IDurableActivityContext activitiyContext)
        {
            if (activitiyContext is null)
                throw new ArgumentNullException(nameof(activitiyContext));

            var functionInput = activitiyContext.GetInput<Input>();

            var project = await projectsRepository
                .GetAsync(functionInput.ProjectId)
                .ConfigureAwait(false);

            if (project is null)
                throw new RetryCanceledException($"Could not find project '{functionInput.ProjectId}'");

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
                        resourceGroup.AddRoleAssignmentAsync(functionInput.PrincipalId.ToString(), AzureRoleDefinition.Contributor),
                        resourceGroup.AddRoleAssignmentAsync(functionInput.PrincipalId.ToString(), AzureRoleDefinition.UserAccessAdministrator) // TODO: do we really need this ???
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

        public struct Input
        {
            public string ProjectId { get; set; }

            public Guid PrincipalId { get; set; }
        }

    }
}
