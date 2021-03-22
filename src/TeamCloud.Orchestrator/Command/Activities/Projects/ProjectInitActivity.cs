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

namespace TeamCloud.Orchestrator.Command.Activities.Projects
{
    public sealed class ProjectInitActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IAzureDeploymentService azureDeploymentService;
        private readonly IAzureSessionService azureSessionService;

        public ProjectInitActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IAzureDeploymentService azureDeploymentService, IAzureSessionService azureSessionService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureDeploymentService = azureDeploymentService ?? throw new System.ArgumentNullException(nameof(azureDeploymentService));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        [FunctionName(nameof(ProjectInitActivity))]
        [RetryOptions(3)]
        public async Task<Project> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var project = context.GetInput<Input>().Project;

            if (!AzureResourceIdentifier.TryParse(project.ResourceId, out var projectResourceId))
            {
                var tenantId = (await azureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

                var organization = await organizationRepository
                    .GetAsync(tenantId.ToString(), project.Organization)
                    .ConfigureAwait(false);

                var session = await azureSessionService
                    .CreateSessionAsync(Guid.Parse(organization.SubscriptionId))
                    .ConfigureAwait(false);

                var resourceGroups = await session.ResourceGroups
                    .ListAsync(loadAllPages: true)
                    .ConfigureAwait(false);

                var resourceGroupName = $"TCP-{project.Slug}-{Math.Abs(Guid.Parse(project.Id).GetHashCode())}";

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

                project.ResourceId = resourceGroup.Id;

                project = await projectRepository
                    .SetAsync(project)
                    .ConfigureAwait(false);

                projectResourceId = AzureResourceIdentifier.Parse(project.ResourceId);
            }

            return project;
        }

        internal struct Input
        {
            public Project Project { get; set; }
        }
    }
}
