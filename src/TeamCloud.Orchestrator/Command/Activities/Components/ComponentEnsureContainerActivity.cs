/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Management.ManagementGroups;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Command.Activities.Components
{
    public sealed class ComponentEnsureContainerActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentEnsureContainerActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IDeploymentScopeRepository deploymentScopeRepository, IAzureResourceService azureResourceService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentEnsureContainerActivity))]
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

            if (!AzureResourceIdentifier.TryParse(component.ResourceId, out var resourceId))
            {
                var deploymentScope = await deploymentScopeRepository
                    .GetAsync(component.Organization, component.DeploymentScopeId)
                    .ConfigureAwait(false);

                component.ResourceId = await CreateResourceIdAsync(component, deploymentScope, log)
                    .ConfigureAwait(false);

                resourceId = AzureResourceIdentifier.Parse(component.ResourceId);
            }

            if (AzureResourceIdentifier.TryParse(component.IdentityId, out var identityId))
            {
                var session = await azureResourceService.AzureSessionService
                    .CreateSessionAsync(identityId.SubscriptionId)
                    .ConfigureAwait(false);

                var identity = await session.Identities
                    .GetByIdAsync(identityId.ToString())
                    .ConfigureAwait(false);

                var roleAssignments = new Dictionary<string, IEnumerable<Guid>>()
                {
                    { identity.PrincipalId, Enumerable.Repeat(AzureRoleDefinition.Contributor, 1) }
                };

                if (string.IsNullOrEmpty(resourceId.ResourceGroup))
                {
                    var subscription = await azureResourceService
                        .GetSubscriptionAsync(resourceId.SubscriptionId, true)
                        .ConfigureAwait(false);

                    await subscription
                        .SetRoleAssignmentsAsync(roleAssignments)
                        .ConfigureAwait(false);
                }
                else
                {
                    var resourceGroup = await azureResourceService
                        .GetResourceGroupAsync(resourceId.SubscriptionId, resourceId.ResourceGroup, true)
                        .ConfigureAwait(false);

                    await resourceGroup
                        .SetRoleAssignmentsAsync(roleAssignments)
                        .ConfigureAwait(false);
                }
            }

            return component;
        }

        private async Task<string> CreateResourceIdAsync(Component component, DeploymentScope deploymentScope, ILogger log)
        {
            var resourceGroupName = $"TCE-{component.Slug}-{Math.Abs(Guid.Parse(component.Id).GetHashCode())}";
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

        private async Task<string> GetComponentLocationAsync(Component component)
        {
            var tenantId = (await azureResourceService.AzureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization)
                .ConfigureAwait(false);

            return organization.Location;
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
                    .HasRoleAssignmentAsync(userObjectId.ToString(), AzureRoleDefinition.Owner, true)
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
