/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ManagementGroups;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
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
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var component = context.GetInput<Input>().Component;

            DeploymentScope deploymentScope = null;

            if (!AzureResourceIdentifier.TryParse(component.ResourceId, out var _))
            {
                deploymentScope ??= await deploymentScopeRepository
                    .GetAsync(component.Organization, component.DeploymentScopeId)
                    .ConfigureAwait(false);

                component.ResourceId = await CreateResourceIdAsync(component, deploymentScope, log).ConfigureAwait(false);


            }

            if (!AzureResourceIdentifier.TryParse(component.IdentityId, out var _))
            {
                deploymentScope ??= await deploymentScopeRepository
                    .GetAsync(component.Organization, component.DeploymentScopeId)
                    .ConfigureAwait(false);

                component.IdentityId = await CreateIdentityIdAsync(component, deploymentScope, log).ConfigureAwait(false);
            }

            component = await EnsureRoleAssignmentsAsync(component, log)
                .ConfigureAwait(false);

            return component;
        }

        private async Task<Component> EnsureRoleAssignmentsAsync(Component component, ILogger log)
        {
            if (AzureResourceIdentifier.TryParse(component.ResourceId, out var componentResourceId))
            {
                var principalID = await GetComponentIdentityIdAsync(component)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(componentResourceId.ResourceGroup))
                {
                    var subscription = await azureResourceService
                        .GetSubscriptionAsync(componentResourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    var isContributor = await subscription
                        .HasRoleAssignmentAsync(principalID, AzureRoleDefinition.Contributor)
                        .ConfigureAwait(false);

                    if (!isContributor)
                    {
                        await subscription
                            .AddRoleAssignmentAsync(principalID, AzureRoleDefinition.Contributor)
                            .ConfigureAwait(false);
                    }
                }
                else
                {
                    var resourceGroup = await azureResourceService
                        .GetResourceGroupAsync(componentResourceId.SubscriptionId, componentResourceId.ResourceGroup)
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
                }
            }

            return component;
        }

        private async Task<string> CreateResourceIdAsync(Component component, DeploymentScope deploymentScope, ILogger log)
        {
            var resourceGroupName = $"TCE-{component.Slug}-{Guid.Parse(component.Id).GetHashCode()}";
            var resourceGroupId = default(string);

            var subscriptionIds = await GetSubscriptionIdsAsync(deploymentScope, log)
                .ConfigureAwait(false);

            if (subscriptionIds.Any())
            {
                var resourceGroupIds = await Task
                    .WhenAll(subscriptionIds.Select(sid => FindResourceId(sid, resourceGroupName)))
                    .ConfigureAwait(false);

                // resourceGroupIds enumeration contains nulls or matches only - and there should be only 1 match
                resourceGroupId = resourceGroupIds.SingleOrDefault(rgid => !string.IsNullOrEmpty(rgid));

                if (string.IsNullOrEmpty(resourceGroupId))
                {
                    var subscriptionIndex = (int)(DateTime.UtcNow.Ticks % subscriptionIds.Count());
                    var subscriptionId = subscriptionIds.Skip(subscriptionIndex).FirstOrDefault();

                    var session = await azureResourceService.AzureSessionService
                        .CreateSessionAsync(subscriptionId)
                        .ConfigureAwait(false);

                    var location = await GetComponentLocationAsync(component)
                        .ConfigureAwait(false);

                    var resourceGroup = await session.ResourceGroups
                        .Define(resourceGroupName)
                            .WithRegion(location)
                            .WithTag("TeamCloud.ComponentId", component.Id)
                        .CreateAsync()
                        .ConfigureAwait(false);

                    resourceGroupId = resourceGroup.Id;
                }
            }
            else
            {
                throw new NotSupportedException($"Unable to allocate resource for component '{component}' as no subscriptions available.");
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

        private async Task<string> GetComponentIdentityIdAsync(Component component)
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


        private async Task<IEnumerable<Guid>> GetSubscriptionIdsAsync(DeploymentScope deploymentScope, ILogger log)
        {
            IEnumerable<Guid> subscriptionIds;

            if (AzureResourceIdentifier.TryParse(deploymentScope.ManagementGroupId, out var managementGroupResourceId))
            {
                try
                {
                    var client = await azureResourceService.AzureSessionService
                        .CreateClientAsync<ManagementGroupsAPIClient>()
                        .ConfigureAwait(false);

                    var group = await client.ManagementGroups
                        .GetAsync(managementGroupResourceId.ResourceTypes.Last().Value, expand: "children")
                        .ConfigureAwait(false);

                    subscriptionIds = group.Children
                        .Where(child => child.Type.Equals("/subscriptions", StringComparison.OrdinalIgnoreCase))
                        .Select(child => Guid.Parse(child.Name));
                }
                catch (Exception exc)
                {
                    log.LogWarning(exc, $"Failed to list available subscriptions from management group {managementGroupResourceId}: {exc.Message}");

                    subscriptionIds = Enumerable.Empty<Guid>();
                }
            }
            else
            {
                try
                {
                    var session = await azureResourceService.AzureSessionService
                        .CreateSessionAsync()
                        .ConfigureAwait(false);

                    var subscriptions = await session.Subscriptions
                        .ListAsync(loadAllPages: true)
                        .ConfigureAwait(false);

                    subscriptionIds = subscriptions
                        .Where(subscription => deploymentScope.SubscriptionIds.Contains(Guid.Parse(subscription.SubscriptionId)))
                        .Select(subscription => Guid.Parse(subscription.SubscriptionId));
                }
                catch (Exception exc)
                {
                    log.LogWarning(exc, $"Failed to list available subscriptions: {exc.Message}");

                    subscriptionIds = Enumerable.Empty<Guid>();
                }
            }

            var identity = await azureResourceService.AzureSessionService
                .GetIdentityAsync()
                .ConfigureAwait(false);

            var subscriptionIdsValidated = await Task
                .WhenAll(subscriptionIds.Select(subscriptionId => ProveOwnershipAsync(subscriptionId, identity.ObjectId)))
                .ConfigureAwait(false);

            return subscriptionIdsValidated
                .Where(subscriptionId => subscriptionId != Guid.Empty);

            async Task<Guid> ProveOwnershipAsync(Guid subscriptionId, Guid userObjectId)
            {
                var subscription = await azureResourceService
                    .GetSubscriptionAsync(subscriptionId)
                    .ConfigureAwait(false);

                var hasOwnership = await subscription
                    .HasRoleAssignmentAsync(userObjectId.ToString(), AzureRoleDefinition.Owner)
                    .ConfigureAwait(false);

                return hasOwnership ? subscriptionId : Guid.Empty;
            }
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
