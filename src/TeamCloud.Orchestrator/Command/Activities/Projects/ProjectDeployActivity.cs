/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Templates;

namespace TeamCloud.Orchestrator.Command.Activities.Projects
{
    public sealed class ProjectDeployActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IAzureDeploymentService azureDeploymentService;
        private readonly IAzureSessionService azureSessionService;

        public ProjectDeployActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IDeploymentScopeRepository deploymentScopeRepository, IAzureDeploymentService azureDeploymentService, IAzureSessionService azureSessionService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        [FunctionName(nameof(ProjectDeployActivity))]
        [RetryOptions(3)]
        public async Task<string> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var project = context.GetInput<Input>().Project;

            var tenantId = (await azureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), project.Organization)
                .ConfigureAwait(false);

            if (!AzureResourceIdentifier.TryParse(project.ResourceId, out var projectResourceId))
            {
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

            var deploymentScopeIds = await deploymentScopeRepository
                .ListAsync(project.Organization)
                .Select(scope => scope.Id)
                .ToArrayAsync()
                .ConfigureAwait(false);

            var template = new ProjectDeployTemplate();

            template.Parameters["deploymentScopeIds"] = deploymentScopeIds;

            var deployment = await azureDeploymentService
                .DeployResourceGroupTemplateAsync(template, Guid.Parse(organization.SubscriptionId), projectResourceId.ResourceGroup)
                .ConfigureAwait(false);

            project.ResourceState = ResourceState.Provisioning;

            project = await projectRepository
                .SetAsync(project)
                .ConfigureAwait(false);

            return deployment.ResourceId;
        }

        internal struct Input
        {
            public Project Project { get; set; }
        }
    }
}
