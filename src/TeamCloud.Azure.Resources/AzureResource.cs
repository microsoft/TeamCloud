/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Azure.OData;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Azure.Resources
{
    public class AzureResource
    {
        internal static async Task<T> InitializeAsync<T>(T azureResource, IAzureResourceService azureResourceService, bool throwIfNotExists)
            where T : AzureResource
        {
            azureResource.AzureResourceService = azureResourceService;

            var exists = await azureResource
                .ExistsAsync()
                .ConfigureAwait(false);

            if (exists)
                return azureResource;

            if (throwIfNotExists)
                throw new AzureResourceException($"Resource {azureResource.ResourceId} not found");

            return null;
        }

        internal AzureResource(string resourceId)
        {
            if (resourceId is null)
                throw new ArgumentNullException(nameof(resourceId));

            ResourceId = AzureResourceIdentifier.Parse(resourceId);
            AzureResourceService = null;
        }

        protected AzureResource(string resourceId, IAzureResourceService azureResourceService)
        {
            if (resourceId is null)
                throw new ArgumentNullException(nameof(resourceId));

            ResourceId = AzureResourceIdentifier.Parse(resourceId);
            AzureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        protected IAzureResourceService AzureResourceService { get; private set; }

        public AzureResourceIdentifier ResourceId { get; }

        public virtual async Task<bool> ExistsAsync()
        {
            var apiVersions = await AzureResourceService
                .GetApiVersionsAsync(ResourceId, true)
                .ConfigureAwait(false);

            var token = await AzureResourceService.AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var response = await ResourceId.GetApiUrl(AzureResourceService.AzureSessionService.Environment)
                .SetQueryParam("api-version", apiVersions.First())
                .WithOAuthBearerToken(token)
                .AllowAnyHttpStatus()
                .GetAsync(completionOption: HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }

        public virtual async Task DeleteAsync(bool deleteLocks = false)
        {
            if (deleteLocks)
            {
                await DeleteLocksAsync(true)
                    .ConfigureAwait(false);
            }

            var apiVersions = await AzureResourceService
                .GetApiVersionsAsync(ResourceId, true)
                .ConfigureAwait(false);

            var token = await AzureResourceService.AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            await ResourceId.GetApiUrl(AzureResourceService.AzureSessionService.Environment)
                .SetQueryParam("api-version", apiVersions.First())
                .WithOAuthBearerToken(token)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .DeleteAsync()
                .ConfigureAwait(false);
        }

        public virtual async Task DeleteLocksAsync(bool waitForDeletion = false)
        {
            var locks = await GetLocksInternalAsync()
                .ConfigureAwait(false);

            if (locks.Any())
            {
                var session = AzureResourceService.AzureSessionService
                    .CreateSession(this.ResourceId.SubscriptionId);

                await session.ManagementLocks
                    .DeleteByIdsAsync(locks.ToArray())
                    .ConfigureAwait(false);

                if (waitForDeletion)
                {
                    var timeoutDuration = TimeSpan.FromMinutes(5);
                    var timeout = DateTime.UtcNow.Add(timeoutDuration);

                    while (DateTime.UtcNow < timeout && locks.Any())
                    {
                        await Task.Delay(5000)
                            .ConfigureAwait(false);

                        locks = await GetLocksInternalAsync()
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<IEnumerable<string>> GetLocksInternalAsync()
        {
            var session = AzureResourceService.AzureSessionService
                .CreateSession(this.ResourceId.SubscriptionId);

            var locks = new List<string>();

            var page = await session.ManagementLocks
                .ListForResourceAsync(this.ResourceId.ToString())
                .ConfigureAwait(false);

            while (page?.Any() ?? false)
            {
                locks.AddRange(page.Select(lck => lck.Id));

                page = await page.GetNextPageAsync()
                    .ConfigureAwait(false);
            }

            return locks.Distinct();
        }

        protected virtual async Task<string> GetLatestApiVersionAsync(bool includePreviewVersions = false)
        {
            var apiVersions = await AzureResourceService
                .GetApiVersionsAsync(this.ResourceId, includePreviewVersions)
                .ConfigureAwait(false);

            return apiVersions.FirstOrDefault();
        }

        private async Task<GenericResourceInner> GetResourceAsync()
        {
            using var resourceManagementClient = AzureResourceService.AzureSessionService
                .CreateClient<ResourceManagementClient>(subscriptionId: this.ResourceId.SubscriptionId);

            var apiVersion = await GetLatestApiVersionAsync()
                .ConfigureAwait(false);

            return await resourceManagementClient.Resources
                .GetByIdAsync(this.ResourceId.ToString(), apiVersion)
                .ConfigureAwait(false);
        }

        private async Task<GenericResourceInner> SetResourceAsync(GenericResourceInner resource)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            using var resourceManagementClient = AzureResourceService.AzureSessionService
                .CreateClient<ResourceManagementClient>(subscriptionId: this.ResourceId.SubscriptionId);

            var apiVersion = await GetLatestApiVersionAsync()
                .ConfigureAwait(false);

            // we need to ensure that the update call doesn't contain
            // a provisioning state information - otherwise the update
            // call will end up in a BAD REQUEST error

            var properties = JObject.FromObject(resource.Properties);
            properties.SelectToken("$.provisioningState")?.Parent.Remove();
            resource.Properties = properties;

            return await resourceManagementClient.Resources
                .UpdateByIdAsync(this.ResourceId.ToString(), apiVersion, resource)
                .ConfigureAwait(false);
        }

        public virtual async Task<IDictionary<string, string>> GetTagsAsync(bool includeHidden = false)
        {
            var resource = await GetResourceAsync()
                .ConfigureAwait(false);

            var resourceTags = resource.Tags ?? new Dictionary<string, string>();

            if (includeHidden)
                return resourceTags;

            return resourceTags
                .Where(kvp => !kvp.Key.StartsWith("hidden-", StringComparison.OrdinalIgnoreCase))
                .ToDictionary();
        }

        public virtual async Task SetTagsAsync(IDictionary<string, string> tags, bool merge = false)
        {
            var resource = await GetResourceAsync()
                .ConfigureAwait(false);

            // we treat hidden tags like system settings
            // and won't delete them - overriding them is OK

            var hiddenTags = resource.Tags
                .Where(kvp => kvp.Key.StartsWith("hidden-", StringComparison.Ordinal))
                .ToDictionary();

            if (merge)
            {
                resource.Tags = resource.Tags.Merge(tags);
            }
            else
            {
                resource.Tags = hiddenTags.Merge(tags);
            }

            _ = await SetResourceAsync(resource)
                .ConfigureAwait(false);
        }

        public virtual async Task<string> GetTagAsync(string key)
        {
            var tags = await GetTagsAsync(true)
                .ConfigureAwait(false);

            if (tags.TryGetValue(key, out string value))
                return value;

            return default;
        }

        public virtual async Task AddTagAsync(string key, string value)
        {
            var resource = await GetResourceAsync()
                .ConfigureAwait(false);

            if (resource.Tags is null)
                resource.Tags = new Dictionary<string, string>();

            resource.Tags.Add(key, value);

            _ = await SetResourceAsync(resource)
                .ConfigureAwait(false);
        }

        public virtual async Task DeleteTagAsync(string key)
        {
            var resource = await GetResourceAsync()
                .ConfigureAwait(false);

            if (resource.Tags?.Any() ?? false)
            {
                var tagCount = resource.Tags.Count;

                resource.Tags = resource.Tags
                    .Where(kvp => kvp.Key.Equals(key, StringComparison.Ordinal))
                    .ToDictionary();

                if (tagCount != resource.Tags.Count)
                {
                    _ = await SetResourceAsync(resource)
                        .ConfigureAwait(false);
                }
            }
        }

        public virtual async Task AddRoleAssignmentAsync(Guid userObjectId, Guid roleDefinitionId)
        {
            using var authClient = AzureResourceService.AzureSessionService
                .CreateClient<AuthorizationManagementClient>();

            var parameters = new RoleAssignmentCreateParameters
            {
                PrincipalId = userObjectId.ToString(),
                RoleDefinitionId = $"/subscriptions/{ResourceId.SubscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleDefinitionId}"
            };

            try
            {
                _ = await authClient
                    .RoleAssignments
                    .CreateAsync(ResourceId.ToString().TrimStart('/'), Guid.NewGuid().ToString(), parameters)
                    .ConfigureAwait(false);
            }
            catch (CloudException exc) when (exc.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // swallow this exception as the role assignment already exists
            }
        }

        public virtual async Task DeleteRoleAssignmentAsync(Guid userObjectId, Guid? roleDefinitionId = null)
        {
            var assignments = await GetRoleAssignmentsInternalAsync(userObjectId)
                .ConfigureAwait(false);

            if (assignments.TryGetValue(userObjectId, out var roleAssignments))
            {
                using var authClient = AzureResourceService.AzureSessionService
                    .CreateClient<AuthorizationManagementClient>();

                var deleteTasks = roleAssignments
                    .Where(roleAssignment => !roleDefinitionId.HasValue || roleAssignment.GetRoleDefinitionId().Equals(roleDefinitionId.Value))
                    .Select(roleAssignment => authClient.RoleAssignments.DeleteByIdAsync(roleAssignment.Id));

                await Task
                    .WhenAll(deleteTasks)
                    .ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> HasRoleAssignmentAsync(Guid userObjectId, Guid roleDefinitionId)
        {
            var assignments = await GetRoleAssignmentsAsync(userObjectId)
                .ConfigureAwait(false);

            return assignments.Contains(roleDefinitionId);
        }

        public virtual async Task<IEnumerable<Guid>> GetRoleAssignmentsAsync(Guid userObjectId)
        {
            var assignments = await GetRoleAssignmentsInternalAsync(userObjectId)
                .ConfigureAwait(false);

            if (assignments.TryGetValue(userObjectId, out var roleAssignments))
                return roleAssignments.Select(roleAssignment => roleAssignment.GetRoleDefinitionId());

            return Enumerable.Empty<Guid>();
        }

        public virtual async Task SetRoleAssignmentsAsync(IDictionary<Guid, IEnumerable<Guid>> roleAssignments)
        {
            if (roleAssignments is null)
                throw new ArgumentNullException(nameof(roleAssignments));

            var assignments = await GetRoleAssignmentsAsync()
                .ConfigureAwait(false);

            var tasks = new List<Task>();

            // delete all role assignments
            // for users that don't exist
            // in our target state

            tasks.AddRange(assignments.Keys
                .Except(roleAssignments.Keys)
                .Select(userObjectId => DeleteRoleAssignmentAsync(userObjectId)));

            // add all role assignments
            // for users that don't exist
            // in our current state

            tasks.AddRange(roleAssignments.Keys
                .Except(assignments.Keys)
                .SelectMany(userObjectId => roleAssignments[userObjectId].Select(roleId => AddRoleAssignmentAsync(userObjectId, roleId))));

            // update role assignments
            // for users existing in
            // our current and target state

            foreach (var userObjectId in assignments.Keys.Intersect(roleAssignments.Keys))
            {
                var currentRoleIds = assignments[userObjectId];
                var targetRoleIds = roleAssignments[userObjectId];

                tasks.AddRange(currentRoleIds
                    .Except(targetRoleIds)
                    .Select(roleId => DeleteRoleAssignmentAsync(userObjectId, roleId)));

                tasks.AddRange(targetRoleIds
                    .Except(currentRoleIds)
                    .Select(roleId => AddRoleAssignmentAsync(userObjectId, roleId)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        public virtual async Task<IDictionary<Guid, IEnumerable<Guid>>> GetRoleAssignmentsAsync()
        {
            var assignments = await GetRoleAssignmentsInternalAsync()
                .ConfigureAwait(false);

            return assignments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(assignment => assignment.GetRoleDefinitionId()));
        }

        private async Task<IDictionary<Guid, IEnumerable<RoleAssignmentInner>>> GetRoleAssignmentsInternalAsync(Guid? userObjectId = null)
        {
            using var authClient = AzureResourceService.AzureSessionService
                .CreateClient<AuthorizationManagementClient>();

            var roles = new List<RoleAssignmentInner>();
            var query = new ODataQuery<RoleAssignmentFilter>();

            if (userObjectId.HasValue)
                query.SetFilter(filter => filter.PrincipalId.Equals(userObjectId.Value.ToString(), StringComparison.OrdinalIgnoreCase));

            var page = await authClient.RoleAssignments
                .ListForScopeAsync(ResourceId.ToString().TrimStart('/'))
                .ConfigureAwait(false);

            roles.AddRange(page);

            while (!string.IsNullOrEmpty(page.NextPageLink))
            {
                page = await authClient.RoleAssignments
                    .ListForScopeNextAsync(page.NextPageLink)
                    .ConfigureAwait(false);

                roles.AddRange(page);
            }

            // ListForScopeAsync includes scopes which encompass the resource, e.g. it returns
            // assignments inherited from the subscription. We filter those out here because we can't
            // modify them.

            return roles
                .Where(role => role.Scope.Equals(ResourceId.ToString(), StringComparison.OrdinalIgnoreCase))
                .GroupBy(role => Guid.Parse(role.PrincipalId))
                .ToDictionary(grp => grp.Key, grp => grp.AsEnumerable());
        }
    }
}
