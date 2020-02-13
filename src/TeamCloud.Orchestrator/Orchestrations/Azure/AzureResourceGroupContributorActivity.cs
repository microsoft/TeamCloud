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
using TeamCloud.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Azure
{
    public class AzureResourceGroupContributorActivity
    {
        private readonly IAzureResourceService azureResourceService;
        private readonly IProjectsRepositoryReadOnly projectsRepository;

        public AzureResourceGroupContributorActivity(IAzureResourceService azureResourceService, IProjectsRepositoryReadOnly projectsRepository)
        {
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(AzureResourceGroupContributorActivity))]
        public async Task RunActivity(
            [ActivityTrigger] (Guid, Guid) projectAndPrincipalId,
            ILogger log)
        {
            var projectId = projectAndPrincipalId.Item1;
            var principalId = projectAndPrincipalId.Item2;

            var project = await projectsRepository
                .GetAsync(projectId)
                .ConfigureAwait(false);

            var resourceGroup = await azureResourceService
                .GetResourceGroupAsync(project.ResourceGroup.SubscriptionId, project.ResourceGroup.ResourceGroupName, throwIfNotExists: true)
                .ConfigureAwait(false);

            await resourceGroup
                .AddRoleAssignmentAsync(principalId, AzureRoleDefinition.Contributor)
                .ConfigureAwait(false);
        }
    }
}
