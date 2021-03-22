/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Command.Activities.Organizations
{
    public sealed class OrganizationInitActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureDeploymentService azureDeploymentService;

        public OrganizationInitActivity(IOrganizationRepository organizationRepository, IAzureSessionService azureSessionService, IAzureDeploymentService azureDeploymentService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureDeploymentService = azureDeploymentService ?? throw new System.ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(OrganizationInitActivity))]
        [RetryOptions(3)]
        public async Task<Organization> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var organization = context.GetInput<Input>().Organization;

            if (!AzureResourceIdentifier.TryParse(organization.ResourceId, out var organizationResourceId))
            {
                var session = await azureSessionService
                    .CreateSessionAsync(Guid.Parse(organization.SubscriptionId))
                    .ConfigureAwait(false);

                var resourceGroups = await session.ResourceGroups
                    .ListAsync(loadAllPages: true)
                    .ConfigureAwait(false);

                var resourceGroupName = $"TCO-{organization.Slug}-{Math.Abs(Guid.Parse(organization.Id).GetHashCode())}";

                var resourceGroup = resourceGroups
                    .SingleOrDefault(rg => rg.Name.Equals(resourceGroupName, StringComparison.OrdinalIgnoreCase));

                if (resourceGroup is null)
                {
                    resourceGroup = await session.ResourceGroups
                        .Define(resourceGroupName)
                            .WithRegion(organization.Location)
                        .CreateAsync()
                        .ConfigureAwait(false);
                }

                organization.ResourceId = resourceGroup.Id;

                organization = await organizationRepository
                    .SetAsync(organization)
                    .ConfigureAwait(false);

            }

            return organization;
        }

        internal struct Input
        {
            public Organization Organization { get; set; }
        }
    }
}
