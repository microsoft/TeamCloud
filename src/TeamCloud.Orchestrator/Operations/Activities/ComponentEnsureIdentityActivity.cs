/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentEnsureIdentityActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentEnsureIdentityActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IDeploymentScopeRepository deploymentScopeRepository, IAzureResourceService azureResourceService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentEnsureIdentityActivity))]
        [RetryOptions(3)]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var component = context.GetInput<Input>().Component;

            if (!AzureResourceIdentifier.TryParse(component.IdentityId, out var _))
            {
                var deploymentScope = await deploymentScopeRepository
                    .GetAsync(component.Organization, component.DeploymentScopeId)
                    .ConfigureAwait(false);

                component.IdentityId = await CreateIdentityIdAsync(component, deploymentScope, log).ConfigureAwait(false);
            }

            return component;
        }

        private async Task<string> CreateIdentityIdAsync(Component component, DeploymentScope deploymentScope, ILogger log)
        {
            var project = await projectRepository
                .GetAsync(component.Organization, component.ProjectId)
                .ConfigureAwait(false);

            var projectResourceId = AzureResourceIdentifier.Parse(project.ResourceId);

            var session = await azureResourceService.AzureSessionService
                .CreateSessionAsync(projectResourceId.SubscriptionId)
                .ConfigureAwait(false);

            var identities = await session.Identities
                .ListByResourceGroupAsync(projectResourceId.ResourceGroup, loadAllPages: true)
                .ConfigureAwait(false);

            var identity = identities
                .SingleOrDefault(i => i.Name.Equals(deploymentScope.Id, StringComparison.OrdinalIgnoreCase));

            if (identity is null)
            {
                var location = await GetComponentLocationAsync(component)
                    .ConfigureAwait(false);

                identity = await session.Identities
                    .Define(deploymentScope.Id)
                        .WithRegion(location)
                        .WithExistingResourceGroup(projectResourceId.ResourceGroup)
                    .CreateAsync()
                    .ConfigureAwait(false);
            }

            return identity.Id;
        }

        private async Task<string> GetComponentLocationAsync(Component component)
        {
            var tenantId = (await azureResourceService.AzureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization)
                .ConfigureAwait(false);

            return organization.Location;
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}