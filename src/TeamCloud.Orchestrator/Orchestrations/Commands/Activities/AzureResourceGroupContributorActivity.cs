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

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
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
            [ActivityTrigger] (Guid projectId, Guid principalId) input,
            ILogger log)
        {
            var project = await projectsRepository
                .GetAsync(input.projectId)
                .ConfigureAwait(false);

            var resourceGroup = await azureResourceService
                .GetResourceGroupAsync(project.ResourceGroup.SubscriptionId, project.ResourceGroup.ResourceGroupName, throwIfNotExists: true)
                .ConfigureAwait(false);

            await resourceGroup
                .AddRoleAssignmentAsync(input.principalId, AzureRoleDefinition.Contributor)
                .ConfigureAwait(false);
        }
    }
}
