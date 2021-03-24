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
using TeamCloud.Data;
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

            var deploymentScopes = await deploymentScopeRepository
                .ListAsync(project.Organization)
                .Select(scope => scope.Id)
                .ToArrayAsync()
                .ConfigureAwait(false);

            var template = new SharedResourcesTemplate();

            //template.Parameters["organizationId"] = organization.Id;
            //template.Parameters["organizationName"] = organization.Slug;
            template.Parameters["projectId"] = project.Id;
            template.Parameters["projectName"] = project.Slug;
            template.Parameters["deploymentScopes"] = deploymentScopes;

            var deployment = await azureDeploymentService
                .DeploySubscriptionTemplateAsync(template, Guid.Parse(organization.SubscriptionId), organization.Location)
                .ConfigureAwait(false);

            return deployment.ResourceId;
        }

        internal struct Input
        {
            public Project Project { get; set; }
        }
    }
}
