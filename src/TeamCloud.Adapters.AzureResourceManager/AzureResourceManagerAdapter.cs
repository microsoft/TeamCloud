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
using Microsoft.Azure.WebJobs.Host;
using Microsoft.VisualStudio.Services.Identity;
using Newtonsoft.Json.Linq;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Azure.Directory;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Secrets;
using TeamCloud.Serialization;
using TeamCloud.Serialization.Forms;

namespace TeamCloud.Adapters.AzureResourceManager
{
    public sealed class AzureResourceManagerAdapter : Adapter
    {
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureResourceService azureResourceService;
        private readonly IOrganizationRepository organizationRepository;
        private readonly IUserRepository userRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;

        public AzureResourceManagerAdapter(IAuthorizationSessionClient sessionClient,
            IAuthorizationTokenClient tokenClient,
            IDistributedLockManager distributedLockManager,
            ISecretsStoreProvider secretsStoreProvider,
            IAzureSessionService azureSessionService,
            IAzureDirectoryService azureDirectoryService,
            IAzureResourceService azureResourceService,
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IDeploymentScopeRepository deploymentScopeRepository,
            IProjectRepository projectRepository,
            IComponentRepository componentRepository,
            IComponentTemplateRepository componentTemplateRepository)
            : base(sessionClient, tokenClient, distributedLockManager, secretsStoreProvider, azureSessionService, azureDirectoryService, organizationRepository, deploymentScopeRepository, projectRepository, userRepository)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
        }

        public override DeploymentScopeType Type
            => DeploymentScopeType.AzureResourceManager;

        public override IEnumerable<ComponentType> ComponentTypes
            => new ComponentType[] { ComponentType.Environment };

        public override async Task<string> GetInputDataSchemaAsync()
        {
            var json = await TeamCloudForm.GetDataSchemaAsync<AzureResourceManagerData>(true)
                .ConfigureAwait(false);

            await Task.WhenAll
            (
                EnhanceSubscriptionIdsAsync(),
                EnhanceManagementGroupIdAsync()

            ).ConfigureAwait(false);

            return json.ToString(Newtonsoft.Json.Formatting.None);

            async Task EnhanceSubscriptionIdsAsync()
            {
                var session = await azureResourceService.AzureSessionService
                    .CreateSessionAsync()
                    .ConfigureAwait(false);

                var subscriptions = await session.Subscriptions
                    .ListAsync(loadAllPages: true)
                    .ConfigureAwait(false);

                if (subscriptions.Any() && json.TrySelectToken("$..properties.subscriptionIds.items", out var subscriptionIdsToken))
                {
                    subscriptionIdsToken["enum"] = new JArray(subscriptions.OrderBy(s => s.DisplayName).Select(s => s.SubscriptionId));
                    subscriptionIdsToken["enumNames"] = new JArray(subscriptions.OrderBy(s => s.DisplayName).Select(s => s.DisplayName));
                    ((JToken)subscriptionIdsToken.Parent.Parent)["uniqueItems"] = new JValue(true);
                }
            }

            async Task EnhanceManagementGroupIdAsync()
            {
                var client = await azureResourceService.AzureSessionService
                    .CreateClientAsync<ManagementGroupsAPIClient>()
                    .ConfigureAwait(false);

                var managementGroupPage = await client.ManagementGroups
                    .ListAsync()
                    .ConfigureAwait(false);

                var managementGroups = await managementGroupPage
                    .AsContinuousCollectionAsync(nextPageLink => client.ManagementGroups.ListNextAsync(nextPageLink))
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (managementGroups.Any() && json.TrySelectToken("$..properties.managementGroupId", out var managementGroupToken))
                {
                    managementGroupToken["enum"] = new JArray(managementGroups.OrderBy(mg => mg.DisplayName).Select(mg => mg.Id));
                    managementGroupToken["enumNames"] = new JArray(managementGroups.OrderBy(mg => mg.DisplayName).Select(mg => mg.DisplayName));
                }
            }
        }

        public override async Task<string> GetInputFormSchemaAsync()
        {
            var json = await TeamCloudForm.GetFormSchemaAsync<AzureResourceManagerData>()
                .ConfigureAwait(false);

            return json.ToString(Newtonsoft.Json.Formatting.None);
        }

        public override Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope)
            => azureSessionService.GetIdentityAsync().ContinueWith(identity => identity != null, TaskScheduler.Current);

        protected override async Task<Component> CreateComponentAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (!AzureResourceIdentifier.TryParse(component.ResourceId, out var resourceId))
            {
                component.ResourceId = await CreateResourceIdAsync(component)
                    .ConfigureAwait(false);

                resourceId = AzureResourceIdentifier.Parse(component.ResourceId);

                var sessionIdenity = await azureResourceService.AzureSessionService
                    .GetIdentityAsync()
                    .ConfigureAwait(false);

                component.ResourceUrl = resourceId.GetPortalUrl(sessionIdenity.TenantId);

                component = await componentRepository
                    .SetAsync(component)
                    .ConfigureAwait(false);
            }

            return await UpdateComponentAsync(component, contextUser, commandQueue).ConfigureAwait(false);
        }

        protected override async Task<Component> UpdateComponentAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (AzureResourceIdentifier.TryParse(component.ResourceId, out var componentResourceId))
            {
                var tasks = new Task[] {
                    UpdateComponentRoleAssignmentsAsync(),
                    UpdateComponentTagsAsync()
                };

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            return component;

            async Task UpdateComponentRoleAssignmentsAsync()
            {
                var roleAssignmentMap = await GetRoleAssignmentsAsync(component)
                    .ConfigureAwait(false);

                if (AzureResourceIdentifier.TryParse(component.IdentityId, out var identityResourceId))
                {
                    var session = await azureResourceService.AzureSessionService
                        .CreateSessionAsync(identityResourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    var identity = await session.Identities
                        .GetByIdAsync(identityResourceId.ToString())
                        .ConfigureAwait(false);

                    roleAssignmentMap
                        .Add(identity.PrincipalId, Enumerable.Repeat(AzureRoleDefinition.Contributor, 1));
                }

                if (string.IsNullOrEmpty(componentResourceId.ResourceGroup))
                {
                    var subscription = await azureResourceService
                        .GetSubscriptionAsync(componentResourceId.SubscriptionId, throwIfNotExists: true)
                        .ConfigureAwait(false);

                    if (subscription != null)
                        await subscription.SetRoleAssignmentsAsync(roleAssignmentMap).ConfigureAwait(false);
                }
                else
                {
                    var resourceGroup = await azureResourceService
                        .GetResourceGroupAsync(componentResourceId.SubscriptionId, componentResourceId.ResourceGroup, throwIfNotExists: true)
                        .ConfigureAwait(false);

                    if (resourceGroup != null)
                        await resourceGroup.SetRoleAssignmentsAsync(roleAssignmentMap).ConfigureAwait(false);
                }
            }

            async Task UpdateComponentTagsAsync()
            {
                var tenantId = await azureResourceService.AzureSessionService
                    .GetTenantIdAsync()
                    .ConfigureAwait(false);

                var organization = await organizationRepository
                    .GetAsync(tenantId.ToString(), component.Organization, true)
                    .ConfigureAwait(false);

                var project = await projectRepository
                    .GetAsync(component.Organization, component.ProjectId, true)
                    .ConfigureAwait(false);

                var tags = organization.Tags
                    .Union(project.Tags)
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(g => g.Key, g => g.First().Value);

                if (string.IsNullOrEmpty(componentResourceId.ResourceGroup))
                {
                    var subscription = await azureResourceService
                        .GetSubscriptionAsync(componentResourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    if (subscription != null)
                        await subscription.SetTagsAsync(tags, true).ConfigureAwait(false);
                }
                else
                {
                    var resourceGroup = await azureResourceService
                        .GetResourceGroupAsync(componentResourceId.SubscriptionId, componentResourceId.ResourceGroup)
                        .ConfigureAwait(false);

                    if (resourceGroup != null)
                        await resourceGroup.SetTagsAsync(tags, true).ConfigureAwait(false);
                }
            }
        }

        protected override async Task<Component> DeleteComponentAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (AzureResourceIdentifier.TryParse(component.ResourceId, out var componentResourceId))
            {
                var resourceGroup = await azureResourceService
                    .GetResourceGroupAsync(componentResourceId.SubscriptionId, componentResourceId.ResourceGroup)
                    .ConfigureAwait(false);

                if (resourceGroup != null)
                {
                    await resourceGroup
                        .DeleteAsync(true)
                        .ConfigureAwait(false);
                }

                // remove resource related informations

                component.ResourceId = null;
                component.ResourceUrl = null;

                // ensure resource state is deleted

                component.ResourceState = Model.Common.ResourceState.Deprovisioned;

                // update entity to ensure we have it's state updated in case the delete fails

                component = await componentRepository
                    .SetAsync(component)
                    .ConfigureAwait(false);
            }

            return component;
        }

        private async Task<string> CreateResourceIdAsync(Component component)
        {
            var resourceGroupName = $"TCE-{component.Slug}-{Math.Abs(Guid.Parse(component.Id).GetHashCode())}";
            var resourceGroupId = default(string);

            var subscriptionIds = await GetSubscriptionIdsAsync(component)
                .ConfigureAwait(false);

            if (subscriptionIds.Any())
            {
                var resourceGroupIds = await Task
                    .WhenAll(subscriptionIds.Select(sid => FindResourceIdAsync(sid, resourceGroupName)))
                    .ConfigureAwait(false);

                // resourceGroupIds enumeration contains nulls or matches only - and there should be only 1 or no match
                resourceGroupId = resourceGroupIds.SingleOrDefault(rgid => !string.IsNullOrEmpty(rgid));

                if (string.IsNullOrEmpty(resourceGroupId))
                {
                    // as Azure subscriptions are limited to 2000 role assignments
                    // we pick those with the least amout of used assignments

                    var leastRoleAssignments = await subscriptionIds
                        .ToAsyncEnumerable()
                        .GroupByAwait(sid => GetRoleAssignmentCountAsync(sid))
                        .OrderBy(cnt => cnt.Key)
                        .FirstOrDefaultAsync()
                        .ConfigureAwait(false);

                    subscriptionIds = leastRoleAssignments.ToEnumerable();

                    // in case there are several subscriptions with the same amount of
                    // role assignments we pick a random one for our deployment

                    var subscriptionIndex = (int)(DateTime.UtcNow.Ticks % subscriptionIds.Count());
                    var subscriptionId = subscriptionIds.Skip(subscriptionIndex).FirstOrDefault();

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
            }
            else
            {
                throw new NotSupportedException($"Unable to allocate resource for component '{component}' as no subscriptions available.");
            }

            return resourceGroupId;

            async ValueTask<int> GetRoleAssignmentCountAsync(Guid subscriptionId)
            {
                var subscription = await azureResourceService
                    .GetSubscriptionAsync(subscriptionId, throwIfNotExists: true)
                    .ConfigureAwait(false);

                var roleAssignmentUsage = await subscription
                    .GetRoleAssignmentUsageAsync()
                    .ConfigureAwait(false);

                return roleAssignmentUsage.RoleAssignmentsCurrentCount;
            }

            async Task<string> FindResourceIdAsync(Guid subscriptionId, string resourceGroupName)
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

        private async Task<string> GetLocationAsync(Component component)
        {
            var tenantId = await azureResourceService.AzureSessionService
                .GetTenantIdAsync()
                .ConfigureAwait(false);

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization)
                .ConfigureAwait(false);

            return organization.Location;
        }

        private async Task<IEnumerable<Guid>> GetSubscriptionIdsAsync(Component component)
        {
            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId, true)
                .ConfigureAwait(false);

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
                catch
                {
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
                catch
                {
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

        private async Task<Dictionary<string, IEnumerable<Guid>>> GetRoleAssignmentsAsync(Component component)
        {
            var session = await azureResourceService.AzureSessionService
                .CreateSessionAsync()
                .ConfigureAwait(false);

            var template = await componentTemplateRepository
                .GetAsync(component.Organization, component.ProjectId, component.TemplateId)
                .ConfigureAwait(false);

            return await userRepository
                .ListAsync(component.Organization, component.ProjectId)
                .SelectAwait(async user => new KeyValuePair<string, IEnumerable<Guid>>(user.Id, await GetUserRoleDefinitionIdsAsync(user).ConfigureAwait(false)))
                .ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value)
                .ConfigureAwait(false);

            async Task<IEnumerable<Guid>> GetUserRoleDefinitionIdsAsync(User user)
            {
                var roleDefinitionIds = new HashSet<Guid>();

                if (template.Permissions?.Any() ?? false)
                {
                    if (user.IsOwner(component.ProjectId) && template.Permissions.TryGetValue(ProjectUserRole.Owner, out var ownerPermissions))
                    {
                        var tasks = ownerPermissions.Select(permission => ResolveRoleDefinitionIdAsync(permission));

                        roleDefinitionIds.UnionWith(await Task.WhenAll(tasks).ConfigureAwait(false));
                    }
                    else if (user.IsAdmin(component.ProjectId) && template.Permissions.TryGetValue(ProjectUserRole.Admin, out var adminPermissions))
                    {
                        var tasks = adminPermissions.Select(permission => ResolveRoleDefinitionIdAsync(permission));

                        roleDefinitionIds.UnionWith(await Task.WhenAll(tasks).ConfigureAwait(false));
                    }
                    else if (user.IsMember(component.ProjectId) && template.Permissions.TryGetValue(ProjectUserRole.Member, out var memberPermissions))
                    {
                        var tasks = memberPermissions.Select(permission => ResolveRoleDefinitionIdAsync(permission));

                        roleDefinitionIds.UnionWith(await Task.WhenAll(tasks).ConfigureAwait(false));
                    }
                }

                // strip out unresolved role defintion ids
                roleDefinitionIds.RemoveWhere(id => id == Guid.Empty);

                if (!roleDefinitionIds.Any())
                {
                    // if no role definition id was resolved use default
                    roleDefinitionIds.Add(AzureRoleDefinition.Reader);
                }

                return roleDefinitionIds;
            }

            async Task<Guid> ResolveRoleDefinitionIdAsync(string permission)
            {
                if (Guid.TryParse(permission, out var roleDefinitionId))
                    return roleDefinitionId;

                try
                {
                    var roleDefinition = await session.RoleDefinitions
                        .GetByScopeAndRoleNameAsync(component.ResourceId, permission)
                        .ConfigureAwait(false);

                    if (roleDefinition is null)
                        return Guid.Empty;

                    return Guid.Parse(roleDefinition.Id.Split('/').LastOrDefault());
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }
    }
}
