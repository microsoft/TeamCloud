/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentInitActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentInitActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IDeploymentScopeRepository deploymentScopeRepository, IAzureResourceService azureResourceService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentInitActivity))]
        [RetryOptions(3)]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var component = context.GetInput<Input>().Component;

            DeploymentScope deploymentScope = null;

            if (!AzureResourceIdentifier.TryParse(component.ResourceId, out var _))
            {
                deploymentScope ??= await deploymentScopeRepository
                    .GetAsync(component.Organization, component.DeploymentScopeId)
                    .ConfigureAwait(false);

                component.ResourceId = await CreateResourceIdAsync(component, deploymentScope).ConfigureAwait(false);
            }

            if (!AzureResourceIdentifier.TryParse(component.IdentityId, out var _))
            {
                deploymentScope ??= await deploymentScopeRepository
                    .GetAsync(component.Organization, component.DeploymentScopeId)
                    .ConfigureAwait(false);

                component.IdentityId = await CreateIdentityIdAsync(component, deploymentScope).ConfigureAwait(false);
            }

            return await EnsureRoleAssignmentsAsync(component)
                .ConfigureAwait(false);
        }

        private async Task<string> GetLocationAsync(Component component)
        {
            var tenantId = (await azureResourceService.AzureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization)
                .ConfigureAwait(false);

            return organization.Location;
        }

        private async Task<string> GetPrincipalIdAsync(Component component)
        {
            var identityResourceId = AzureResourceIdentifier.Parse(component.IdentityId);

            var session = await azureResourceService.AzureSessionService
                .CreateSessionAsync(identityResourceId.SubscriptionId)
                .ConfigureAwait(false);

            var identity = await session.Identities
                .GetByIdAsync(identityResourceId.ToString())
                .ConfigureAwait(false);

            return identity.PrincipalId;
        }

        private async Task<Component> EnsureRoleAssignmentsAsync(Component component)
        {
            var componentResourceId = AzureResourceIdentifier.Parse(component.ResourceId);

            var resourceGroup = await azureResourceService
                .GetResourceGroupAsync(componentResourceId.SubscriptionId, componentResourceId.ResourceGroup)
                .ConfigureAwait(false);

            var principalID = await GetPrincipalIdAsync(component)
                .ConfigureAwait(false);

            var isContributor = await resourceGroup
                .HasRoleAssignmentAsync(principalID, AzureRoleDefinition.Contributor)
                .ConfigureAwait(false);

            if (!isContributor)
            {
                await resourceGroup
                    .AddRoleAssignmentAsync(principalID, AzureRoleDefinition.Contributor)
                    .ConfigureAwait(false);
            }

            return component;
        }

        private async Task<string> CreateResourceIdAsync(Component component, DeploymentScope deploymentScope)
        {
            var resourceGroupName = $"TCE-{component.Slug}-{Guid.Parse(component.Id).GetHashCode()}";

            var resourceGroupIds = await Task
                .WhenAll(deploymentScope.SubscriptionIds.Select(sid => FindResourceId(sid, resourceGroupName)))
                .ConfigureAwait(false);

            var resourceGroupId = resourceGroupIds.SingleOrDefault(rgid => !string.IsNullOrEmpty(rgid));

            if (string.IsNullOrEmpty(resourceGroupId))
            {
                var subscriptionIndex = DateTime.UtcNow.Ticks % deploymentScope.SubscriptionIds.Count;
                var subscriptionId = deploymentScope.SubscriptionIds[(int)subscriptionIndex];

                var session = await azureResourceService.AzureSessionService
                    .CreateSessionAsync(subscriptionId)
                    .ConfigureAwait(false);

                var location = await GetLocationAsync(component)
                    .ConfigureAwait(false);

                var resourceGroup = await session.ResourceGroups
                    .Define(resourceGroupName)
                        .WithRegion(location)
                        .WithTag("TeamCloud.ComponentId", component.Id)
                    .CreateAsync()
                    .ConfigureAwait(false);

                resourceGroupId = resourceGroup.Id;
            }

            return resourceGroupId;

            async Task<string> FindResourceId(Guid subscriptionId, string resourceGroupName)
            {
                var session = await azureResourceService.AzureSessionService
                    .CreateSessionAsync(subscriptionId)
                    .ConfigureAwait(false);

                try
                {
                    var resourceGroup = await session.ResourceGroups
                        .GetByNameAsync(resourceGroupName)
                        .ConfigureAwait(false);

                    return resourceGroup?.Id;
                }
                catch
                {
                    return null;
                }
            }
        }

        private async Task<string> CreateIdentityIdAsync(Component component, DeploymentScope deploymentScope)
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
                var location = await GetLocationAsync(component)
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

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
