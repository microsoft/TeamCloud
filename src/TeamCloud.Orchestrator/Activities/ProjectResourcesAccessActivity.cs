/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.KeyVault.Fluent.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Data;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectResourcesAccessActivity
    {
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureResourceService azureResourceService;
        private readonly IProjectsRepository projectsRepository;

        public ProjectResourcesAccessActivity(IAzureSessionService azureSessionService, IAzureResourceService azureResourceService, IProjectsRepository projectsRepository)
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
                     .GetResourceGroupAsync(project.ResourceGroup.SubscriptionId, project.ResourceGroup.Name, throwIfNotExists: true)
                     .ConfigureAwait(false);

                if (resourceGroup != null)
                {
                    var roleAssignments = await resourceGroup
                        .GetRoleAssignmentsAsync(principalId.ToString())
                        .ConfigureAwait(false);

                    if (!roleAssignments.Contains(AzureRoleDefinition.Contributor))
                    {
                        await resourceGroup
                            .AddRoleAssignmentAsync(principalId.ToString(), AzureRoleDefinition.Contributor)
                            .ConfigureAwait(false);
                    }

                    if (!roleAssignments.Contains(AzureRoleDefinition.UserAccessAdministrator))
                    {
                        await resourceGroup
                            .AddRoleAssignmentAsync(principalId.ToString(), AzureRoleDefinition.UserAccessAdministrator)
                            .ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
